using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nexmo.Api;
using NexmoPSEDemo.Common;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using static Nexmo.Api.NumberInsight;

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
            ViewData["RegistrationStatus"] = "new";
            return View();
        }

        [HttpPost]
        public IActionResult Index(RegistrationModel viewModel, string verifyAction)
        {
            if (ModelState.IsValid)
            {
                // create a logger
                Logger logger = NexmoLogger.GetLogger("RegistrationLogger"); ;

                try
                {
                    if (verifyAction == "Register")
                    {
                        if (logger == null)
                        {
                            logger = NexmoLogger.GetLogger("RegistrationLogger");
                        }
                        logger.Open();

                        if (string.IsNullOrEmpty(viewModel.PinCode) && viewModel.Workflow == "1")
                        {
                            var results = NexmoApi.SendVerifyRequest(viewModel, logger, configuration);
                            if (results.status == "0")
                            {
                                logger.Log("Verify request successfully created with requestId: " + results.request_id);
                                ViewData["feedback"] = "Thanks " + viewModel.Name + ". We have sent a verification code to the number you provided.";
                                ViewData["requestId"] = results.request_id;
                                ViewData["number"] = viewModel.Number;
                                ViewData["model"] = "0";
                                ViewData["RegistrationStatus"] = "started";
                            }
                            else if (results.status == "10")
                            {
                                ViewData["warning"] = "Please wait for the previous request to complete, then try again.";
                                logger.Log(Level.Warning, "Response code: " + results.status + " - Concurrent verifications to the same number are not allowed. Request ID: " + results.request_id);
                            }
                            else
                            {
                                ViewData["error"] = "Your request could not be created at this time. Please try again later.";
                                logger.Log(Level.Warning, "Response code: " + results.status + " - Request could not be completed. Request ID: " + results.request_id + " - Error Text: " + results.error_text);
                            }
                        }
                        else
                        {
                            // If a different workflow than the default has been selected but the PIN code is not generated in the UI
                            // then generate a random PIN
                            if (string.IsNullOrEmpty(viewModel.PinCode))
                            {
                                var random = new Random();
                                string pin = random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();
                                pin += random.Next(0, 9).ToString();

                                viewModel.PinCode = pin;
                            }

                            var results = NexmoApi.VerifyRequest(viewModel, logger, configuration);
                            var response = JsonConvert.DeserializeObject<VerifyResponse>(results);

                            if (response.status == "0")
                            {
                                logger.Log("Verify request successfully created with requestId: " + response.request_id);
                                ViewData["feedback"] = "Thanks " + viewModel.Name + ". We have sent a verification code to the number you provided.";
                                ViewData["requestId"] = response.request_id;
                                ViewData["number"] = viewModel.Number;
                                ViewData["model"] = "2";
                                ViewData["RegistrationStatus"] = "started";
                            }
                            else if (response.status == "10")
                            {
                                ViewData["warning"] = "Please wait for the previous request to complete, then try again.";
                                logger.Log(Level.Warning, "Response code: " + response.status + " - Concurrent verifications to the same number are not allowed. Request ID: " + response.request_id);
                            }
                            else
                            {
                                ViewData["error"] = "Your request could not be created at this time. Please try again later.";
                                logger.Log(Level.Warning, "Response code: " + response.status + " - Request could not be completed. Request ID: " + response.request_id + " - Error Text: " + response.error_text);
                            }
                        }
                    }
                    else if (verifyAction == "Check")
                    {
                        if (logger == null)
                        {
                            logger = NexmoLogger.GetLogger("RegistrationLogger");
                        }
                        logger.Open();

                        string pinCode = viewModel.PinCode;
                        string requestId = viewModel.RequestId;
                        string number = viewModel.Recipient;
                        var results = new NumberVerify.CheckResponse();

                        if(viewModel.Model == "2") // pay per request pricing model
                        {
                            results = NexmoApi.CheckVerify(viewModel, logger, configuration, requestId);
                        }
                        else
                        {
                            results = NexmoApi.CheckVerifyRequest(viewModel, logger, configuration, requestId);
                        }

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
                            ViewData["RegistrationStatus"] = "completed";

                            // send confirmation message
                            var messagingModel = new MessagingModel()
                            {
                                Sender = viewModel.Name,
                                Number = viewModel.Number,
                                Text = "Your account has been created successfully. You can access it here: http://dashboard.nexmo.com"
                            };
                            var smsResults = NexmoApi.SendSMS(messagingModel, configuration, "60");
                            foreach (SMS.SMSResponseDetail responseDetail in smsResults.messages)
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
                            ViewData["warning"] = "Your code could not be verified. Please try again.";
                            logger.Log(Level.Warning, "The code could not be verified with status: " + results.status + " and message: " + results.error_text);
                            ViewData["RegistrationStatus"] = "started";
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["error"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    if (logger != null)
                    {
                        logger.Close();
                        logger.Deregister();
                    }
                }
            }

            return View(viewModel);
        }

        public IActionResult Soft()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Soft(ValidationModel validationModel)
        {
            if (ModelState.IsValid)
            {
                // create a logger
                Logger logger = NexmoLogger.GetLogger("SoftValidationLogger"); ;

                try
                {
                    if (logger == null)
                    {
                        logger = NexmoLogger.GetLogger("SoftValidationLogger");
                    }
                    logger.Open();

                    NumberInsightAdvancedResponse results = NexmoApi.AdvancedNumberInsightRequest(validationModel, configuration);
                    if (results.Status == "0" && results.NumberValidity == "valid")
                    {
                        logger.Log("Soft validation with NI advanced request successfully created with requestId: " + results.RequestId);
                        var responseObject = JsonConvert.SerializeObject(NexmoApi.GenerateAdvancedObject(results), Formatting.Indented);
                        ViewData["feedback"] = "Thanks " + validationModel.Name + ". We have checked your phone number and your account has been validated. More details: \n" + responseObject;
                    }
                    else if (results.NumberValidity != "valid")
                    {
                        logger.Log("Soft validation with NI advanced request failed with number validity: " + results.NumberValidity + " for requestId: " + results.RequestId);
                        ViewData["feedback"] = "Thanks " + validationModel.Name + ". Unfortunately it seems that the number you provided is invalid. Please try again with a different phone number.";
                    }
                    else
                    {
                        ViewData["error"] = "Your request could not be completed at this time. Please try again later.";
                        logger.Log(Level.Exception, "Response code: " + results.Status + " - Request could not be completed. Request ID: " + results.RequestId + " - Error Text: " + results.ErrorText);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(Level.Exception, e);
                    ViewData["error"] = "There has been an issue dealing with your request. Please try again later.";
                }
                finally
                {
                    if (logger != null)
                    {
                        logger.Close();
                        logger.Deregister();
                    }
                }
            }

            return View(validationModel);
        }

    }
}
