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
    public class RegistrationController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(RegistrationModel viewModel, string verifyAction)
        {
            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = NexmoLogger.GetLogger("RegistrationLogger"); ;

                try
                {
                    if (verifyAction == "Register")
                    {
                        if( logger == null){
                            logger = NexmoLogger.GetLogger("RegistrationLogger");
                        }
                        logger.Open();

                        var results = NexmoApi.SendVerifyRequest(viewModel, logger, configuration);
                        if (results.status == "0")
                        {
                            logger.Log("Verify request successfully created with requestId: " + results.request_id);
                            ViewData["feedback"] = "Thanks " + viewModel.Name + ". We have sent a verification code to the number you provided.";
                            ViewData["requestId"] = results.request_id;
                            ViewData["number"] = viewModel.Number;
                        }
                        else if (results.status == "10")
                        {
                            ViewData["feedback"] = "Please wait for the previous request to complete, then try again.";
                        }
                        else
                        {
                            ViewData["feedback"] = "Your request could not be created at this time. Please try again later.";
                        }
                    }
                    else if(verifyAction == "Check")
                    {
                        if (logger == null)
                        {
                            logger = NexmoLogger.GetLogger("RegistrationLogger");
                        }
                        logger.Open();

                        string pinCode = viewModel.PinCode;
                        string requestId = viewModel.RequestId;
                        string number = viewModel.Recipient; 
                        var results = NexmoApi.CheckVerifyRequest(viewModel, logger, configuration, requestId);

                        // log the request response for future debugging
                        string response = "Response returned with status code: " + results.status;
                        response += " and error text: " + results.error_text;
                        response += " and price: " + results.price + " " + results.currency;
                        response += " and eventId: " + results.event_id;
                        logger.Log(response);

                        if (results.status == "0")
                        {
                            // provide feedback on the page
                            ViewData["feedback"] = "Your code has been successfully verified.";
                            logger.Log("PIN code: " + pinCode + " successfully verified. We have sent an confirmation message to the number provided.");

                            // send confirmation message
                            var smsResults = NexmoApi.SendSMS(number, configuration, "Your account has been created successfully. You can access it here: http://dashboard.nexmo.com", "Nexmo PSE", "60");
                            foreach(SMS.SMSResponseDetail responseDetail in smsResults.messages)
                            {
                                string messageDetails = "SMS sent successfully with messageId: " + responseDetail.message_id;
                                messageDetails += " for Verify requestId: " + requestId;
                                messageDetails += " to: " + responseDetail.to;
                                messageDetails += " at price: " + responseDetail.message_price;
                                messageDetails += " with status: " + responseDetail.status;
                                logger.Log(messageDetails);
                            }
                        }
                        else
                        {
                            ViewData["feedback"] = "Your code could not be verified. Please try again.";
                            logger.Log(Level.Warning, "The code could not be verified with status: " + results.status + " and message: " + results.error_text);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["feedback"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    //if (logger != null)
                    //{
                        //logger.Close();
                        //logger.Deregister();
                    //}
                }
            }

            return View(viewModel);
        }
    }
}
