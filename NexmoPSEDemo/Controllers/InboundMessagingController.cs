using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Nexmo.Api;
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

        // SMS endpoints
        [HttpGet]
        [Route("messaging/sms/queue/next")]
        public string NexmoSMS()
        {
            // create a logger placeholder
            Logger logger = null;
            var inboundSMS = new InboundSmsObject();
            var messageResult = string.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("MessagingSmsQueueLogger");
                logger.Open();

                var queue = Storage.CreateQueue("chat", configuration, logger);
                var message = Storage.GetNextMessage(queue, logger);

                if (message != null)
                {
                    messageResult = message.AsString;
                    inboundSMS = JsonConvert.DeserializeObject<InboundSmsObject>(message.AsString);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return "Error";
            }

            return messageResult;
        }

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

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
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
                        if (moSmsObject.text.ToLower().Trim() == "trigger")
                        {
                            VoiceModel voiceModel = new VoiceModel()
                            {
                                From = moSmsObject.to,
                                To = moSmsObject.msisdn
                            };

                            var alertNcco = NexmoApi.MakeAlertTTSCall(voiceModel, logger, configuration);
                            if (alertNcco)
                            {
                                httpRequest.CreateResponse(HttpStatusCode.UnprocessableEntity);
                            }
                        }
                        else if (moSmsObject.text.ToLower().Trim() == "rob")
                        {
                            var result = NexmoApi.MakeIvrCallWithMachineDetection(moSmsObject.text, logger, configuration);
                        }
                        else if (moSmsObject.text.ToLower().Trim() == "mason")
                        {
                            var result = NexmoApi.MakeIvrCallWithMachineDetection(moSmsObject.text, logger, configuration);
                        }
                        else if (moSmsObject.text.ToLower().Trim() == "kaine")
                        {
                            var result = NexmoApi.MakeIvrCallWithMachineDetection(moSmsObject.text, logger, configuration);
                        }
                        else if (moSmsObject.text.ToLower().Trim() == "perry")
                        {
                            var result = NexmoApi.MakeIvrCallWithMachineDetection(moSmsObject.text, logger, configuration);
                        }
                        else if (moSmsObject.text.ToLower().Trim() == "jpc")
                        {
                            var result = NexmoApi.MakeIvrCallWithMachineDetection(moSmsObject.text, logger, configuration);
                        }
                        else
                        {
                            // Add the message in a queue to be processed in the chat demo
                            var queue = Storage.CreateQueue("chat", configuration, logger);
                            Storage.InsertMessageInQueue(queue, JsonConvert.SerializeObject(moSmsObject), 3000, logger);

                            logger.Log(Level.Warning, "Messaging SMS Inbound added to the queue: " + JsonConvert.SerializeObject(moSmsObject, Formatting.Indented));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("messaging/sms/send")]
        public HttpResponseMessage SendSms()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("MessagingSendSmsLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var chatSmsObject = JsonConvert.DeserializeObject<ChatSmsObject>(value.Result);
                    logger.Log("Messaging Send SMS Chat body: " + JsonConvert.SerializeObject(chatSmsObject, Formatting.Indented));
                    logger.Log("Messaging Send SMS Chat - The text message entered is: " + chatSmsObject.Text);
                    logger.Log("Messaging Send SMS Chat - The text message recipient is: " + chatSmsObject.To);

                    if (!string.IsNullOrEmpty(chatSmsObject.Text))
                    {
                        var message = new MessagingModel()
                        {
                            Sender = configuration["appSettings:Nexmo.Application.Number.From.UK"],
                            Number = chatSmsObject.To,
                            Text = chatSmsObject.Text
                        };

                        var smsResults = NexmoApi.SendSMS(message, configuration, "");
                        foreach (SMS.SMSResponseDetail responseDetail in smsResults.messages)
                        {
                            string messageDetails = "SMS sent successfully with messageId: " + responseDetail.message_id;
                            messageDetails += " \n to: " + responseDetail.to;
                            messageDetails += " \n at price: " + responseDetail.message_price;
                            messageDetails += " \n with status: " + responseDetail.status;
                            logger.Log(messageDetails);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(HttpStatusCode.OK);
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

        [HttpGet]
        [Route("messaging/wa/queue/next")]
        public string NexmoWA()
        {
            // create a logger placeholder
            Logger logger = null;
            var inboundSMS = new InboundWAObject();
            var messageResult = string.Empty;

            try
            {
                logger = NexmoLogger.GetLogger("MessagingWAQueueLogger");
                logger.Open();

                var queue = Storage.CreateQueue("wachat", configuration, logger);
                var message = Storage.GetNextMessage(queue, logger);

                if (message != null)
                {
                    messageResult = message.AsString;
                    inboundSMS = JsonConvert.DeserializeObject<InboundWAObject>(message.AsString);
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return "Error";
            }

            return messageResult;
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

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var fmObject = JsonConvert.DeserializeObject<FMRootObject>(value.Result);
                    logger.Log("Messaging Inbound body: " + JsonConvert.SerializeObject(fmObject, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("messaging/wa/inbound")]
        public HttpResponseMessage WAInbound()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("InboundMessagingLogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var waInboundObject = JsonConvert.DeserializeObject<InboundWAObject>(value.Result);
                    logger.Log("Messaging WA Inbound body: " + JsonConvert.SerializeObject(waInboundObject, Formatting.Indented));

                    // Add the message in a queue to be processed in the wa chat demo
                    var queue = Storage.CreateQueue("wachat", configuration, logger);
                    Storage.InsertMessageInQueue(queue, JsonConvert.SerializeObject(waInboundObject), 3000, logger);

                    logger.Log(Level.Warning, "Messaging SMS Inbound added to the queue: " + JsonConvert.SerializeObject(waInboundObject, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("messaging/wa/send")]
        public HttpResponseMessage SendWA()
        {
            // create a logger placeholder
            Logger logger = null;
            var httpRequest = new HttpRequestMessage();

            try
            {
                logger = NexmoLogger.GetLogger("MessagingSendWALogger");
                logger.Open();

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var value = reader.ReadToEndAsync();
                    var chatWAObject = JsonConvert.DeserializeObject<ChatWAObject>(value.Result);
                    logger.Log("Messaging Send WA Chat body: " + JsonConvert.SerializeObject(chatWAObject, Formatting.Indented));
                    logger.Log("Messaging Send WA Chat - The text message entered is: " + chatWAObject.Text);
                    logger.Log("Messaging Send WA Chat - The text message recipient is: " + chatWAObject.To);

                    if (!string.IsNullOrEmpty(chatWAObject.Text))
                    {
                        var message = new MessagingModel()
                        {
                            Sender = "447418342149",
                            Number = chatWAObject.To,
                            Text = chatWAObject.Text,
                            Type = "WhatsApp",
                            ContentType = "text"
                        };

                        if(NexmoApi.SendMessage(message, logger, configuration))
                        {
                            return httpRequest.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return httpRequest.CreateResponse(HttpStatusCode.FailedDependency);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e);
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return httpRequest.CreateResponse(HttpStatusCode.OK);
        }
    }
}
