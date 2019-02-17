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
using NexmoPSEDemo.Models;
using NSpring.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NexmoPSEDemo.Controllers
{
    
    public class InboundVoiceController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        // GET: vapi/<controller>
        [HttpGet]
        [Route("vapi/[controller]")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET vapi/<controller>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST vapi/status
        [HttpPost]
        [Route("vapi/status")]
        public HttpResponseMessage Status()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("VoiceStatusLogger");
                logger.Open();

                var headers = Request.Headers;
                var host = headers["Host"];
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Voice Status update from: " + host);
                    logger.Log("Voice Status update body: " + value.Result);
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

        // POST vapi/inbound
        [HttpPost]
        [Route("vapi/inbound")]
        public string Inbound()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("InboundVoiceLogger");
                logger.Open();

                var headers = Request.Headers;
                var host = headers["Host"];
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var voiceObject = JsonConvert.DeserializeObject<FMRootObject>(value.Result);
                    logger.Log("Voice Inbound from: " + host);
                    logger.Log("Voice Inbound body: " + JsonConvert.SerializeObject(voiceObject, Formatting.Indented));
                }

                //httpRequest = Common.NexmoApi.AnswerVoiceCall(logger, configuration);
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                //return httpRequest.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            //return httpRequest.CreateResponse(System.Net.HttpStatusCode.OK);
            string ncco = Common.NexmoApi.AnswerVoiceCall(logger, configuration);
            return ncco;
        }

        // PUT vapi/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE vapi/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
