﻿
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nexmo.Api;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using static Nexmo.Api.NumberVerify;
using static Nexmo.Api.SMS;

namespace NexmoPSEDemo.Common
{
    public static class Configuration
    {
        public static IConfigurationRoot GetConfigFile()
        {
            string configFile;
#if DEBUG
            // use this for Mac OS
            configFile = "/Volumes/GoogleDrive/My Drive/Documents/nexmo/visual studio apps/Nexmo PSE Demo/nexmo-presales/NexmoPSEDemo/appsettings.json";
            // use this for PC
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                configFile = "C:\\Users\\jchenot\\OneDrive - Nexmo\\Applications\\visual studio apps\\Nexmo PSE Demo\\nexmo-presales\\NexmoPSEDemo\\NexmoPSEDemo\\appsettings.json";
            }
#else
            configFile = "appsettings.json";
#endif
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configFile);
            builder.AddUserSecrets<Startup>();
            IConfigurationRoot configuration = builder.Build();

            return configuration;
        }
    }

    public static class NexmoLogger
    {
        public static Logger GetLogger(string loggerName)
        {
            string logDirectory;
            // Make sure the directory to write the log files to exists
#if DEBUG
            // use this for Mac OS
            logDirectory = "/Volumes/GoogleDrive/My Drive/Documents/nexmo/visual studio apps/Nexmo PSE Demo/nexmo-presales/NexmoPSEDemo/Logs/";
            // use this for PC
            if(Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                logDirectory = "C:\\Users\\jchenot\\OneDrive - Nexmo\\Applications\\visual studio apps\\Nexmo PSE Demo\\nexmo-presales\\NexmoPSEDemo\\Logs\\";
            }
#else
            logDirectory = "../../LogFiles/Application/";
#endif
            // Make sure the directory to write the log files to exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create and configure the file logger
            var fileLogger = Logger.CreateFileLogger(logDirectory + "demo_log-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ".txt");
            fileLogger.IsArchivingEnabled = true;
            fileLogger.MaximumFileSize = 1024;

            // Make sure the directory to archive the log files to exists
            // use this for Mac OS
            string archiveDirectory = logDirectory + "Archive/";
            // use this for PC
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                archiveDirectory = logDirectory + "Archive\\";
            }

            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }
            fileLogger.ArchiveDirectoryPath = archiveDirectory;

            // Set up the new instance of the file logger and return it
            Logger.CloseLoggers();
            Logger.AddLogger(loggerName, fileLogger);
            Logger logger = Logger.GetLogger(loggerName);

            return logger;
        }
    }

    public static class NexmoApi
    {
        public static VerifyResponse SendVerifyRequest(RegistrationModel viewModel, Logger logger, IConfigurationRoot configuration)
        {
            var verifyRequest = new NumberVerify.VerifyRequest
            {
                number = viewModel.Number,
                brand = "Nexmo PSE Demo",
                sender_id = "Nexmo PSE",
                pin_expiry = "60",
                next_event_wait = "60"
            };

            // log the request parameters for future debugging
            string verifyRequestParams = "Verify request created with number: " + verifyRequest.number;
            verifyRequestParams += " and brand: " + verifyRequest.brand;
            verifyRequestParams += " and senderId: " + verifyRequest.sender_id;
            verifyRequestParams += " and pinExpiry: " + verifyRequest.pin_expiry;
            verifyRequestParams += " and nextEventWait: " + verifyRequest.next_event_wait;

            logger.Log(verifyRequestParams);

            // trigger Verify API request
            var client = GenerateNexmoClient(configuration);
            return client.NumberVerify.Verify(request: verifyRequest);
        }

        public static CheckResponse CheckVerifyRequest(RegistrationModel viewModel, Logger logger, IConfigurationRoot configuration, string requestId)
        {
            var checkRequest = new NumberVerify.CheckRequest
            {
                code = viewModel.PinCode,
                request_id = requestId
            };

            // log the request parameters for future debugging
            logger.Log("Making a PIN check request with requestId: " + checkRequest.request_id + " and PIN code: " + checkRequest.code);

            var client = GenerateNexmoClient(configuration);

            return client.NumberVerify.Check(request: checkRequest);
        }

        public static SMSResponse SendSMS(MessagingModel messagingModel, IConfigurationRoot configuration, string pinExpiry)
        {
            var client = GenerateNexmoClient(configuration);

            return client.SMS.Send(request: new SMS.SMSRequest
            {
                to = messagingModel.Number,
                text = messagingModel.Text,
                from = messagingModel.Sender
            });
        }

        public static bool SendMessage(MessagingModel messagingModel, Logger logger, IConfigurationRoot configuration)
        {
            // extract the url and token from the configuration file
            string url = configuration["appSettings:Nexmo.Url.Api"] + "/v0.1/messages";
            if (messagingModel.Type == "WhatsApp")
                url = configuration["appSettings:Nexmo.Url.WA.Sandbox"];
            string token = configuration["Nexmo:Messaging.WA.Token"]; // TODO: replace with input from web user???

            // get the json object to pass in the request
            string messageObj = GenerateMessageJson(messagingModel);

            // start creating the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if(messagingModel.Type == "WhatsApp")
                request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                logger.Log(messageObj);
                request.Content = new StringContent(messageObj, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(Level.Info, response.StatusCode.ToString());
                        logger.Log(Level.Info, response.RequestMessage);
                        logger.Log(Level.Info, response.Headers);
                        logger.Log(Level.Info, response.Content);
                    }
                    else
                    {
                        logger.Log(Level.Warning, response.StatusCode.ToString());
                        logger.Log(Level.Warning, response.ReasonPhrase);

                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);

                return false;
            }

            return true;
        }

        public static bool SendDispatchFailover(FailoverModel failoverModel, Logger logger, IConfigurationRoot configuration)
        {
            // extract the url and token from the configuration file
            string url = configuration["appSettings:Nexmo.Url.Api"] + "/v0.1/dispatch";
            string token = configuration["appSettings:Nexmo.Messaging.WA.Token"]; // TODO: replace with input from web user???

            // get the json object to pass in the request
            string rootObj = GenerateDispatchApiJson(failoverModel);

            // start creating the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                logger.Log(rootObj);
                request.Content = new StringContent(rootObj, Encoding.UTF8, "application/json");

                using(var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(Level.Info, response.StatusCode.ToString());
                        logger.Log(Level.Info, response.RequestMessage);
                        logger.Log(Level.Info, response.Headers);
                        logger.Log(Level.Info, response.Content);
                    }
                    else
                    {
                        logger.Log(Level.Warning, response.StatusCode.ToString());
                        logger.Log(Level.Warning, response.ReasonPhrase);

                        return false;
                    }
                }                
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);

                return false;
            }

            return true;
        }

        private static Client GenerateNexmoClient(IConfigurationRoot configuration)
        {
            var client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = configuration["Nexmo:Api.Key"],
                ApiSecret = configuration["Nexmo:Api.Secret"],
                AppUserAgent = configuration["appSettings:Nexmo.UserAgent"]
            });

            return client;
        }

        private static string GenerateMessageJson(MessagingModel messagingModel)
        {
            string sender = "447418342149";
            if (messagingModel.Type != "WhatsApp")
                sender = messagingModel.Sender;

            var messageJson = new MessagingObject()
            {
                From = new From() { Type = messagingModel.Type, Number = sender },
                To = new To() { Type = messagingModel.Type, Number = messagingModel.Number },
                Message = new Message()
                {
                    Content = new Content()
                    {
                        Type = messagingModel.ContentType,
                        Text = messagingModel.Text
                    }
                }
            };

            return JsonConvert.SerializeObject(messageJson).ToLower();
        }

        private static string GenerateDispatchApiJson(FailoverModel failoverModel)
        {
            // build the json body of the request
            From waFrom = new From()
            {
                Type = "whatsapp",
                Number = "447418342149" // TODO: replace with a number provided by the user?
            };
            To waTo = new To()
            {
                Type = "whatsapp",
                Number = "447843608441" // TODO: replace with input from web user
            };
            Message waMessage = new Message()
            {
                Content = new Content()
                {
                    Type = "text", // TODO: replace with selection from web user
                    Text = "This is the WA from the failover flow" // TODO: replace with input from web user
                }
            };
            Failover failover = new Failover()
            {
                Expiry_time = 60, // TODO: replace with input from web user
                Condition_status = "read" // TODO: replace with selection from web user
            };
            From smsFrom = new From()
            {
                Type = "sms",
                Number = "447418342149" // TODO: replace with a sender provided by the user
            };
            To smsTo = new To()
            {
                Type = "sms",
                Number = "447843608441" // TODO: replace with input from web user
            };
            Message smsMessage = new Message()
            {
                Content = new Content()
                {
                    Type = "text", // TODO: replace with selection from web user
                    Text = "This is the SMS from the failover flow" // TODO: replace with input from web user
                }
            };
            Workflow workflow = new Workflow()
            {
                From = waFrom,
                To = waTo,
                Message = waMessage,
                Failover = failover,
                From2 = smsFrom,
                To2 = smsTo,
                Message2 = smsMessage
            };

            // bring everything together in the root object to make the request
            DispatchRootObject rootObj = new DispatchRootObject()
            {
                Template = "failover",
                Workflow = workflow
            };

            string jsonObj = "{\"template\":\"failover\",";
            jsonObj += "\"workflow\": [";
            jsonObj += "{\"from\": { \"type\": \"whatsapp\", \"number\": \"447418342149\" },";
            jsonObj += "\"to\": { \"type\": \"whatsapp\", \"number\": \"447843608441\" },";
            jsonObj += "\"message\": {";
            jsonObj += "\"content\": {";
            jsonObj += "\"type\": \"text\",";
            jsonObj += "\"text\": \"This is a WhatsApp Message sent via the Dispatch API\"}},";
            jsonObj += "\"failover\":{";
            jsonObj += "\"expiry_time\": 60,";
            jsonObj += "\"condition_status\": \"read\"}},";
            jsonObj += "{\"from\": {\"type\": \"sms\", \"number\": \"JPC Failover Test\"},";
            jsonObj += "\"to\": { \"type\": \"sms\", \"number\": \"447843608441\"},";
            jsonObj += "\"message\": {";
            jsonObj += "\"content\": {";
            jsonObj += "\"type\": \"text\",";
            jsonObj += "\"text\": \"This is an SMS sent via the Dispatch API\"}}}]}";

            return jsonObj;
        }
    }
}