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
        public IActionResult Index(SMSModel messagingModel)
        {
            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = null;

                try
                {
                    logger = NexmoLogger.GetLogger("SMSLogger");
                    logger.Open();

                    var smsResults = NexmoApi.SendSMS(messagingModel.Number, configuration, messagingModel.Message, messagingModel.Sender, "");
                    foreach (SMS.SMSResponseDetail responseDetail in smsResults.messages)
                    {
                        string messageDetails = "SMS sent successfully with messageId: " + responseDetail.message_id;
                        messageDetails += " to: " + responseDetail.to;
                        messageDetails += " at price: " + responseDetail.message_price;
                        messageDetails += " with status: " + responseDetail.status;
                        logger.Log(messageDetails);
                        ViewData["feedback"] = messageDetails;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["feedback"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    logger.Close();
                    logger.Deregister();
                }
            }

            return View();
        }
    }
}
