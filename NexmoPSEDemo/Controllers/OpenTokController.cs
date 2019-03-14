using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        [Route("ot/session/create")]
        public string CreateSession()
        {
            // create a logger placeholder
            Logger logger = null;

            var httpRequest = new HttpRequestMessage();
            var value = string.Empty;

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
                    var callStatus = JsonConvert.DeserializeObject<CallStatus>(value, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    logger.Log("OpenTok Session Creation body: " + JsonConvert.SerializeObject(callStatus, Formatting.Indented));
                }

                // Create an opentok session
                var session = openTok.CreateSession("", MediaMode.ROUTED, ArchiveMode.MANUAL);
                var sessionName = JsonConvert.DeserializeObject<SessionName>(value);
                var clientSession = new ClientSession
                {
                    ApiKey = session.ApiKey.ToString(),
                    ApiSecret = session.ApiSecret,
                    ArchiveMode = session.ArchiveMode.ToString(),
                    Id = session.Id,
                    Location = session.Location,
                    MediaMode = session.MediaMode.ToString(),
                    Name = sessionName.Name
                };

                logger.Log("Session created with session ID: " + session.Id);
                logger.Log("Session object: " + JsonConvert.SerializeObject(session, Formatting.Indented));

                // Create an opentok token
                clientSession.Token = session.GenerateToken();
                logger.Log("Client Session Object generated with token: " + JsonConvert.SerializeObject(clientSession, Formatting.Indented));

                var clientSessionJson = JsonConvert.SerializeObject(clientSession, Formatting.Indented);
                return clientSessionJson;
            }
            catch (Exception e)
            {
                logger.Log("Error creating a session: " + e.Message);
                logger.Log("Error creating a session: " + e.StackTrace);

                return e.Message;
            }
        }
    }
}