using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using NexmoPSEDemo.Common;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using OpenTokSDK;

namespace NexmoPSEDemo.Controllers
{
    public class OpenTokController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Configuration.GetConfigFile();

        // GET: api/<controller>
        [HttpGet]
        [Route("ot/logs")]
        public IEnumerable<string> GetLogs()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        [Route("ot/session/monitoring")]
        public HttpResponseMessage SessionMonitoring()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();
            string sessionEvent = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("SessionMonitoringLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    sessionEvent = value.Result;
                    logger.Log("OpenTok Session Monitoring body: " + JsonConvert.SerializeObject(sessionEvent, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("ot/session/create")]
        public string CreateSession()
        {
            // create a logger placeholder
            Logger logger = null;

            var httpRequest = new HttpRequestMessage();
            var value = string.Empty;
            var blobName = "openTokSessions";
            var sessionName = new SessionName();
            var sessionExist = false;
            var clientSession = new ClientSession();
            List<ClientSession> sessions = new List<ClientSession>();

            var apiKey = configuration["appSettings:OT.Api.Key"];
            var apiSecret = configuration["appSettings:OT.Api.Secret"];

            var openTok = new OpenTok(Convert.ToInt32(apiKey), apiSecret);

            try
            {
                logger = NexmoLogger.GetLogger("OTSessionLogger");
                logger.Open();

                // Get the session name out of the request body
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    value = reader.ReadToEndAsync().Result;
                    sessionName = JsonConvert.DeserializeObject<SessionName>(value);
                    logger.Log("OpenTok Session Creation Request body: " + value);
                }

                // Check if the session name already exists
                // Get an instance of the blob storage to store the session data
                CloudBlobContainer container = Storage.GetCloudBlobContainer("opentok-container");
                logger.Log("Blob container created if it does not exist: " + container.CreateIfNotExistsAsync().Result.ToString());

                string sessionBlobs = Storage.GetBlob(blobName, "opentok-container");
                if (sessionBlobs.StartsWith('['))
                {
                    sessions = JsonConvert.DeserializeObject<List<ClientSession>>(sessionBlobs);
                }
                else if(sessionBlobs.StartsWith('{'))
                {
                    sessions.Add(JsonConvert.DeserializeObject<ClientSession>(sessionBlobs));
                }

                if (sessions.Count > 0)
                {
                    foreach (ClientSession sessionBlob in sessions)
                    {
                        if (sessionBlob.Name == sessionName.Name)
                        {
                            sessionExist = true;
                            clientSession = sessionBlob;

                            // Create an opentok token
                            clientSession.Token = openTok.GenerateToken(sessionBlob.Id);

                            var clientSessionJsonFormatted = JsonConvert.SerializeObject(clientSession, Formatting.Indented);
                            logger.Log("OpenTok Client Session already exist: " + clientSessionJsonFormatted);
                        }
                    }
                }

                if (!sessionExist)
                {
                    // Create an opentok session
                    var session = openTok.CreateSession("", MediaMode.ROUTED, ArchiveMode.MANUAL);

                    clientSession.ApiKey = session.ApiKey.ToString();
                    clientSession.ApiSecret = session.ApiSecret;
                    clientSession.ArchiveMode = session.ArchiveMode.ToString();
                    clientSession.Id = session.Id;
                    clientSession.Location = session.Location;
                    clientSession.MediaMode = session.MediaMode.ToString();
                    clientSession.Name = sessionName.Name;
                    // Create an opentok token
                    clientSession.Token = session.GenerateToken();

                    // Add the session to the blobs to be uploaded
                    sessions.Add(clientSession);

                    logger.Log("OpenTok Session created with session ID: " + session.Id);
                    logger.Log("OpenTok Session object: " + JsonConvert.SerializeObject(session, Formatting.Indented));
                    logger.Log("OpenTok Client Session Object generated with token: " + JsonConvert.SerializeObject(clientSession, Formatting.Indented));

                    NexmoApi.StoreOpenTokData("opentok-container", logger, JsonConvert.SerializeObject(sessions), blobName);
                }


                var clientSessionJson = JsonConvert.SerializeObject(clientSession);
                return clientSessionJson;
            }
            catch (Exception e)
            {
                logger.Log("OpenTok Error creating a session: " + e.Message);
                logger.Log("OpenTok Error creating a session: " + e.StackTrace);

                return e.Message;
            }

        }

        [HttpPost]
        [Route("ot/session/archive/start")]
        public string RecordSession()
        {
            // create a logger placeholder
            Logger logger = null;

            var value = string.Empty;
            var archiveRequest = new ArchiveRequest();

            var apiKey = configuration["appSettings:OT.Api.Key"];
            var apiSecret = configuration["appSettings:OT.Api.Secret"];

            var openTok = new OpenTok(Convert.ToInt32(apiKey), apiSecret);

            try
            {
                logger = NexmoLogger.GetLogger("OTSessionArchivingLogger");
                logger.Open();

                // Get the session name out of the request body
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    value = reader.ReadToEndAsync().Result;
                    archiveRequest = JsonConvert.DeserializeObject<ArchiveRequest>(value);
                    logger.Log("OpenTok Session Recording Request body: " + value);
                }

                // Start the recording
                var archive = openTok.StartArchive(archiveRequest.sessionId);

                logger.Log("OpenTok Session archiving started for session ID: " + archiveRequest.sessionId);
                logger.Log("OpenTok Session archiving started with archive ID: " + archive.Id);
                logger.Log("OpenTok Session archiving started with parameters: " + JsonConvert.SerializeObject(archive, Formatting.Indented));

                // store the archive ID to be able to stop the recording later
                StoreArchiveID(logger, archive);

                var archiveId = archive.Id;
                return archiveId.ToString();
            }
            catch (Exception e)
            {
                logger.Log("OpenTok Error starting archiving the session: " + e.Message);
                logger.Log("OpenTok Error starting archiving the session: " + e.StackTrace);

                return e.Message;
            }
        }

        private void StoreArchiveID(Logger logger, OpenTokSDK.Archive archive)
        {
            var blobName = "openTokArchives";
            List<OpenTokSDK.Archive> archives = new List<OpenTokSDK.Archive>();

            // Get an instance of the blob storage to store the session data
            CloudBlobContainer container = Storage.GetCloudBlobContainer("opentok-container");
            logger.Log("Blob container created if it does not exist: " + container.CreateIfNotExistsAsync().Result.ToString());

            string archiveBlobs = Storage.GetBlob(blobName, "opentok-container");
            if (archiveBlobs.StartsWith('['))
                archives = JsonConvert.DeserializeObject<List<OpenTokSDK.Archive>>(archiveBlobs);
            else if (archiveBlobs.StartsWith('{'))
                archives.Add(JsonConvert.DeserializeObject<OpenTokSDK.Archive>(archiveBlobs));

            if (archives.Where(s => s.Id == archive.Id).Any())
            {
                var archiveJsonFormatted = JsonConvert.SerializeObject(archives.Where(s => s.Id == archive.Id).FirstOrDefault(), Formatting.Indented);
                logger.Log("OpenTok archive already exist: " + archiveJsonFormatted);
            }
            else
            {
                // Add the session to the blobs to be uploaded
                archives.Add(archive);

                logger.Log("OpenTok Archive created with session ID: " + archive.SessionId);
                logger.Log("OpenTok Archive created with archive ID: " + archive.Id);
                logger.Log("OpenTok Archive object: " + JsonConvert.SerializeObject(archive, Formatting.Indented));

                NexmoApi.StoreOpenTokData("opentok-container", logger, JsonConvert.SerializeObject(archives), blobName);
            }
        }

        [HttpPost]
        [Route("ot/session/archive/stop")]
        public string StopRecordingSession()
        {
            // create a logger placeholder
            Logger logger = null;

            var value = string.Empty;
            var stopArchiveRequest = new StopArchiveRequest();

            var apiKey = configuration["appSettings:OT.Api.Key"];
            var apiSecret = configuration["appSettings:OT.Api.Secret"];

            var openTok = new OpenTok(Convert.ToInt32(apiKey), apiSecret);

            try
            {
                logger = NexmoLogger.GetLogger("OTSessionArchivingLogger");
                logger.Open();

                // Get the session name out of the request body
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    value = reader.ReadToEndAsync().Result;
                    stopArchiveRequest = JsonConvert.DeserializeObject<StopArchiveRequest>(value);
                    logger.Log("OpenTok Session Recording end Request body: " + value);
                }

                // Get the archive ID from the session ID
                var blobName = "openTokArchives";
                CloudBlobContainer container = Storage.GetCloudBlobContainer("opentok-container");
                string archiveBlobs = Storage.GetBlob(blobName, "opentok-container");
                var archives = JsonConvert.DeserializeObject<List<OpenTokSDK.Archive>>(archiveBlobs);

                if (archives.Count > 0)
                    stopArchiveRequest.archiveId = archives.Where(a => a.SessionId == stopArchiveRequest.sessionId).FirstOrDefault().Id;
                else
                    return "There is no archive for this session.";

                // Start the recording
                var archive = openTok.StopArchive(stopArchiveRequest.archiveId.ToString());

                logger.Log("OpenTok Session archiving ended for session ID: " + archive.SessionId);
                logger.Log("OpenTok Session archiving ended with archive ID: " + archive.Id);

                var archiveId = archive.Id;
                return archiveId.ToString();
            }
            catch (Exception e)
            {
                logger.Log("OpenTok Error ending archiving the session: " + e.Message);
                logger.Log("OpenTok Error ending archiving the session: " + e.StackTrace);

                return e.Message;
            }
        }
    }
}