using System;
using System.Collections.Generic;
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

namespace NexmoPSEDemo.Controllers
{
    public class VoiceController : Controller
    {
        // Load the configuration file
        IConfigurationRoot configuration = Common.Configuration.GetConfigFile();

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(VoiceModel voiceModel)
        {
            if (ModelState.IsValid)
            {
                // create a logger placeholder
                Logger logger = null;

                try
                {
                    if (NexmoApi.MakeVoiceCall(voiceModel, logger, configuration))
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
            }

            return View();
        }

        public IActionResult Alarm()
        {
            ViewData["From.FR"] = configuration["appSettings:Nexmo.Application.Number.From.FR"];
            ViewData["From.UK"] = configuration["appSettings:Nexmo.Application.Number.From.UK"];

            return View();
        }

        [HttpPost]
        public IActionResult Alarm(VoiceModel voiceModel)
        {
            return View();
        }
    }
}