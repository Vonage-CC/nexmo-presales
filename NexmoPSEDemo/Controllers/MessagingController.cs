using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexmo.Api;
using NexmoPSEDemo.Common;
using NexmoPSEDemo.Models;
using NSpring.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NexmoPSEDemo.Controllers
{
    public class MessagingController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Index(MessagingModel messagingModel)
        {
            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = null;

                try
                {
                    logger = NexmoLogger.GetLogger("MessagingLogger");
                    logger.Open();

                    // TODO: Update the content type based on content send (e.g. video, audio, text, etc...)
                    messagingModel.ContentType = "text";

                    switch (messagingModel.Type)
                    {
                        case "WhatsApp":
                            messagingModel.TemplateName = "whatsapp:hsm:technology:nexmo:simplewelcome";
                            if (NexmoApi.SendMessage(messagingModel, logger, configuration))
                                ViewData["feedback"] = "Your " + messagingModel.Type + " message was sent succesfully.";
                            else
                                ViewData["error"] = "We could not send your " + messagingModel.Type + " message. Please try again later.";
                            break;
                        case "Viber":
                            break;
                        case "Facebook Messenger":
                            break;
                        default:
                            var smsResults = NexmoApi.SendSMS(messagingModel, configuration, "");
                            foreach (SMS.SMSResponseDetail responseDetail in smsResults.messages)
                            {
                                string messageDetails = "SMS sent successfully with messageId: " + responseDetail.message_id;
                                messageDetails += " \n to: " + responseDetail.to;
                                messageDetails += " \n at price: " + responseDetail.message_price;
                                messageDetails += " \n with status: " + responseDetail.status;
                                logger.Log(messageDetails);
                                ViewData["feedback"] = messageDetails;
                            }

                            break;
                    }
                    
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["error"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    logger.Close();
                    logger.Deregister();
                }
            }

            return View();
        }

        // GET: /<controller>/
        public IActionResult Failover()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Failover(FailoverModel failoverModel)
        {
            // create a logger
            Logger logger = NexmoLogger.GetLogger("SMSLogger");
            logger.Open();

            if (ModelState.IsValid)
            {
                try
                {
                    // send the message with failover
                    if (NexmoApi.SendDispatchFailover(failoverModel, logger, configuration))
                        ViewData["feedback"] = "Your message has been sent successfully.";
                    else
                        ViewData["error"] = "We could not send your message. Please try again later";
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["error"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    logger.Close();
                    logger.Deregister();
                }
            }
            else
            {
                logger.Log(Level.Warning, "Model State: " + ModelState.ValidationState);
                logger.Log(Level.Warning, "Model State Values: " + ModelState.Values);
            }

            return View();
        }
    }
}
