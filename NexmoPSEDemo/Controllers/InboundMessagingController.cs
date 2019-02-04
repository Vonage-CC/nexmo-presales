using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NexmoPSEDemo.Common;
using NSpring.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NexmoPSEDemo.Controllers
{
    public class InboundMessagingController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        // GET: api/<controller>
        [HttpGet]
        [Route("api/[controller]")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        [Route("api/status")]
        public HttpResponseMessage Status()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("MessagingStatusLogger");
                logger.Open();

                var headers = Request.Headers;
                var host = headers["Host"];
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Status update from: " + host);
                    logger.Log("Status update body: " + value.Result);
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
        [Route("api/inbound")]
        public HttpResponseMessage Inbound()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("InboundMessagingLogger");
                logger.Open();

                var headers = Request.Headers;
                var host = headers["Host"];
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Inbound from: " + host);
                    logger.Log("Inbound body: " + value.Result);
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

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Route("api/[controller]")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Route("api/[controller]")]
        public void Delete(int id)
        {
        }
    }
}
