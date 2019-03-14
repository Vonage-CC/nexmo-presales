using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using NexmoPSEDemo.Common;
using NexmoPSEDemo.Models;
using NSpring.Logging;

namespace NexmoPSEDemo.Controllers
{
    public class VoiceController : Controller
    {
        // Load the configuration file
        IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        public IActionResult Index()
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            return View();
        }

        [HttpPost]
        public IActionResult Index(VoiceModel voiceModel)
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = null;

                try
                {
                    logger = NexmoLogger.GetLogger("TTSLogger");
                    logger.Open();

                    if (NexmoApi.MakeBasicTTSCall(voiceModel, logger, configuration))
                    {
                        ViewData["feedback"] = "Your phone call is starting now...";                        
                    }
                    else
                    {
                        ViewData["error"] = "Your request could not be connected at this time. Please try again later.";
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

        public IActionResult Alarm()
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            // create a logger placeholder
            Logger logger = null;

            try
            {
                logger = NexmoLogger.GetLogger("VoiceAlarmLogger");
                logger.Open();

                // Get an instance of the blob storage to store the phone number to connect to
                CloudBlobContainer container = Storage.GetCloudBlobContainer();
                ViewData["feedback"] = "Create a blob container if it does not exist: " + container.CreateIfNotExistsAsync().Result.ToString() + " \n";
                ViewData["feedback"] += "The storage container has been loaded successfully. \n";
                logger.Log(container.Name + " has been loaded successfully.");

                var blobUpload = Storage.UploadBlobAsync(container, logger, string.Empty, "alarmAlert");
                ViewData["feedback"] += "The recipient's phone number has been reset successfully.";
                logger.Log("The recipient's phone number has been reset successfully.");
            }
            catch
            {
                ViewData["error"] = "The recipient's phone number could not be reset. Please try again.";
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return View();
        }

        [HttpPost]
        public IActionResult Alarm(VoiceModel voiceModel)
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            // create a logger placeholder
            Logger logger = null;

            try
            {
                logger = NexmoLogger.GetLogger("VoiceAlarmLogger");
                logger.Open();

                // Get an instance of the blob storage to store the phone number to connect to
                CloudBlobContainer container = Storage.GetCloudBlobContainer();
                ViewData["feedback"] = "Create a blob container if it does not exist: " + container.CreateIfNotExistsAsync().Result.ToString() + " \n";
                ViewData["feedback"] += "The storage container has been loaded successfully. \n";
                logger.Log(container.Name + " has been loaded successfully.");

                var blobUpload = Storage.UploadBlobAsync(container, logger, voiceModel.To, "alarmAlert");
                ViewData["feedback"] += "The recipient's phone number " + voiceModel.To + " has been saved successfully.";
                logger.Log("The recipient's phone number " + voiceModel.To + " has been saved successfully.");
            }
            catch
            {
                ViewData["error"] = "The recipient's phone number " + voiceModel.To + " could not be saved. Please try again.";
            }
            finally
            {
                logger.Close();
                logger.Deregister();
            }

            return View();
        }

        public IActionResult VoiceAssistant()
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            return View();
        }
    }
}