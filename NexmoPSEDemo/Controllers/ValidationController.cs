using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nexmo.Api;
using NexmoPSEDemo.Common;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using System;
using static Nexmo.Api.NumberInsight;

namespace NexmoPSEDemo.Controllers
{
    public class ValidationController : Controller
    {
        // load the configuration file to access Nexmo's API credentials
        readonly IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ValidationModel validationModel)
        {
            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = null;

                try
                {
                    logger = NexmoLogger.GetLogger("ValidationLogger");
                    logger.Open();

                    switch (validationModel.Version)
                    {
                        case "basic":
                            NumberInsightBasicResponse response = NexmoApi.BasicNumberInsightRequest(validationModel, configuration);

                            if (response.Status == "0")
                            {
                                var responseObject = JsonConvert.SerializeObject(NexmoApi.GenerateBasicObject(response), Formatting.Indented);
                                logger.Log("Request ID: " + response.RequestId + " has completed successfully with status code: " + response.Status + " and status text: " + response.StatusMessage);
                                ViewData["feedback"] = "Your request completed successfully. Please see below the response: " + responseObject;
                            }
                            break;
                        case "standard":
                            NumberInsightStandardResponse standardResponse = NexmoApi.StandardNumberInsightRequest(validationModel, configuration);

                            if (standardResponse.Status == "0")
                            {
                                var responseObject = JsonConvert.SerializeObject(NexmoApi.GenerateStandardObject(standardResponse), Formatting.Indented);
                                logger.Log("Request ID: " + standardResponse.RequestId + " has completed successfully with status code: " + standardResponse.Status + " and status text: " + standardResponse.StatusMessage);
                                ViewData["feedback"] = "Your request completed successfully. Please see below the response: " + responseObject;
                            }
                            break;
                        case "advanced":
                            NumberInsightAdvancedResponse advancedResponse = NexmoApi.AdvancedNumberInsightRequest(validationModel, configuration);

                            if (advancedResponse.Status == "0")
                            {
                                var responseObject = JsonConvert.SerializeObject(NexmoApi.GenerateAdvancedObject(advancedResponse), Formatting.Indented);
                                logger.Log("Request ID: " + advancedResponse.RequestId + " has completed successfully with status code: " + advancedResponse.Status + " and status text: " + advancedResponse.StatusMessage);
                                ViewData["feedback"] = "Your request completed successfully. Please see below the response: " + responseObject;
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
    }
}