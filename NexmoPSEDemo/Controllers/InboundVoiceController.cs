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

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var callStatus = JsonConvert.DeserializeObject<CallStatus>(value.Result, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    logger.Log("Voice Status update body: " + JsonConvert.SerializeObject(callStatus, Formatting.Indented));

                    if (callStatus.status == "machine")
                    {
                        // Transfer the call to the answer machine message
                        if(NexmoApi.TransferCall(logger, configuration))
                        {
                            return httpRequest.CreateResponse(System.Net.HttpStatusCode.OK);
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

        // GET vapi/input/ivr
        [HttpGet]
        [Route("vapi/onanswer")]
        public string OnAnswer()
        {
            // create a logger placeholder
            Logger logger = null;
            string ncco = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("OnAnswerVoiceLogger");
                logger.Open();

                var queryString = Request.QueryString.Value;
                var queryStringList = queryString.Split('&');

                var from = queryStringList[1].Split('=')[1];
                var uuid = queryStringList[3].Split('=')[1];
                var con_uuid = queryStringList[2].Split('=')[1];

                logger.Log("On Answer Input Query String: " + queryString);
                ncco = NexmoApi.CallWhisperTalkAction(logger, configuration);
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return ncco;
        }

        // POST vapi/inbound
        [HttpPost]
        [Route("vapi/inbound")]
        public string Inbound()
        {
            // create a logger placeholder
            Logger logger = null;
            string ncco = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("InboundVoiceLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Voice inbound request content: " + value.Result);
                    var voiceInboundObject = JsonConvert.DeserializeObject<VoiceInboundObject>(value.Result);
                    logger.Log("Voice Inbound body: " + JsonConvert.SerializeObject(voiceInboundObject, Formatting.Indented));

                    ncco = NexmoApi.AnswerVoiceAssistantCall(voiceInboundObject, logger, configuration);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, "Voice Inbound Exception", e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return ncco;
        }

        // POST vapi/transfer
        [HttpPost]
        [Route("vapi/transfer")]
        public string Transfer()
        {
            // create a logger placeholder
            Logger logger = null;
            var ncco = string.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("TransferCallLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("Call transfered answer machine message request content: " + value.Result);

                    // Return NCCO with answer machine message
                    ncco = NexmoApi.AnswerMachineMessageNcco(logger, configuration);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, "Transfer Call Exception", e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return ncco;
        }

        [Route("vapi/asrassistant")]
        public string Asr()
        {
            // create a logger placeholder
            Logger logger = null;
            string ncco = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("AsrVoiceLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    logger.Log("ASR input raw request content: " + value.Result);
                    var asrInputObject = JsonConvert.DeserializeObject<AsrInputObject>(value.Result);
                    logger.Log("ASR input body: " + JsonConvert.SerializeObject(asrInputObject, Formatting.Indented));

                    var voiceInputObject = new VoiceInputObject()
                    {
                        Conversation_uuid = asrInputObject.conversation_uuid,
                        Uuid = asrInputObject.uuid,
                        Dtmf = "1",
                        Timed_out = true,
                        Timestamp = asrInputObject.timestamp
                    };
                    ncco = NexmoApi.AnswerVoiceCallInput(voiceInputObject, logger, configuration);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, "ASR Inbound Exception", e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return ncco;
        }

        // POST vapi/input
        [HttpPost]
        [Route("vapi/input")]
        public string Input()
        {
            // create a logger placeholder
            Logger logger = null;
            string ncco = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("InputVoiceLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var voiceInputObject = JsonConvert.DeserializeObject<VoiceInputObject>(value.Result);
                    logger.Log("Voice Input body: " + JsonConvert.SerializeObject(voiceInputObject, Formatting.Indented));
                    ncco = NexmoApi.AnswerVoiceCallInput(voiceInputObject, logger, configuration);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return ncco;
        }

        // POST vapi/input/ivr
        [HttpPost]
        [Route("vapi/input/ivr")]
        public string InputIvrMachineDetection()
        {
            // create a logger placeholder
            Logger logger = null;
            string ncco = String.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("InputIvrMachineDetectionVoiceLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var voiceInputObject = JsonConvert.DeserializeObject<VoiceInputObject>(value.Result);
                    logger.Log("Voice IVR Machine Detection Input body: " + JsonConvert.SerializeObject(voiceInputObject, Formatting.Indented));
                    ncco = NexmoApi.AnswerVoiceCallInputIvrMachineDetection(voiceInputObject, logger, configuration);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

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
