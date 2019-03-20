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
        //[HttpGet("{id}")]
        //[Route("messaging/sms/{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST messaging/sms/status
        [HttpPost]
        [Route("messaging/sms/status")]
        public HttpResponseMessage SmsStatus()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("MessagingSmsStatusLogger");
                logger.Open();

                var headers = Request.Headers;
                var host = headers["Host"];
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Messaging SMS Status update from: " + host);
                    logger.Log("Messaging SMS Status update body: " + value.Result);
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
        [Route("messaging/sms/inbound")]
        public HttpResponseMessage SmsInbound()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("InboundMessagingSmsLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var moSmsObject = JsonConvert.DeserializeObject<InboundSmsObject>(value.Result);
                    logger.Log("Messaging SMS Inbound body: " + JsonConvert.SerializeObject(moSmsObject, Formatting.Indented));
                    logger.Log("Messaging SMS Inbound - The text message entered is: " + moSmsObject.text);
                    logger.Log("Messaging SMS Inbound - The text message reciptient is: " + moSmsObject.to);

                    if (moSmsObject.to == configuration["appSettings:Nexmo.Application.Number.From.FR"] || moSmsObject.to == configuration["appSettings:Nexmo.Application.Number.From.UK"])
                    {
                        if(moSmsObject.text.ToLower().Trim() == "trigger")
                        {
                            VoiceModel voiceModel = new VoiceModel()
                            {
                                From = moSmsObject.to,
                                To = moSmsObject.msisdn
                            };
                            var alertNcco = NexmoApi.MakeAlertTTSCall(voiceModel, logger, configuration);
                            if (alertNcco)
                            {
                                httpRequest.CreateResponse(System.Net.HttpStatusCode.UnprocessableEntity);
                            }
                        }
                        else
                        {
                            logger.Log(Level.Warning, "Messaging SMS Inbound: The text message entered was: " + moSmsObject.text);
                        }
                    }
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

        // Messaging endpoints
        [HttpPost]
        [Route("messaging/status")]
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
                    logger.Log("Messaging Status update from: " + host);
                    logger.Log("Messaging Status update body: " + value.Result);
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
        [Route("messaging/inbound")]
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
                    var fmObject = JsonConvert.DeserializeObject<FMRootObject>(value.Result);
                    logger.Log("Messaging Inbound from: " + host);
                    logger.Log("Messaging Inbound body: " + JsonConvert.SerializeObject(fmObject, Formatting.Indented));
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
