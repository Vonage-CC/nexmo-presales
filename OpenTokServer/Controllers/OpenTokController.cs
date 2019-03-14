using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using OpenTokSDK;
using OpenTokServer.Common;
using NSpring.Logging;
using System.IO;
using System.Web.Http;
using System.Text;
using Newtonsoft.Json;
using OpenTokServer.Models;

namespace OpenTokServer.Controllers
{
    public class OpenTokController : ApiController
    {
        // GET: api/<controller>
        [HttpGet]
        [Route("ot/logs")]
        public IEnumerable<string> GetLogs()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        [Route("ot/createsession")]
        public string CreateSession()
        {
            // create a logger placeholder
            Logger logger = null;

            var httpRequest = new HttpRequestMessage();

            var apiKey = ConfigurationManager.AppSettings["OT.Api.Key"];
            var apiSecret = ConfigurationManager.AppSettings["OT.Api.Secret"];

            var openTok = new OpenTok(Convert.ToInt32(apiKey), apiSecret);

            try
            {
                logger = NexmoLogger.GetLogger("OTSessionLogger");
                logger.Open();

                // Get the session name out of the request body
                var value = Request.Content.ReadAsStringAsync().Result;
                logger.Log("OpenTok Session Creation body: " + value);

                // Create an opentok session
                var session = openTok.CreateSession("", MediaMode.ROUTED, ArchiveMode.MANUAL);
                var sessionName = JsonConvert.DeserializeObject<SessionName>(value);
                var clientSession = new ClientSession();

                clientSession.ApiKey = session.ApiKey.ToString();
                clientSession.ApiSecret = session.ApiSecret;
                clientSession.ArchiveMode = session.ArchiveMode.ToString();
                clientSession.Id = session.Id;
                clientSession.Location = session.Location;
                clientSession.MediaMode = session.MediaMode.ToString();
                clientSession.Name = sessionName.Name;

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
