
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Nexmo.Api;
using Nexmo.Api.Voice;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using static Nexmo.Api.NumberInsight;
using static Nexmo.Api.NumberVerify;
using static Nexmo.Api.SMS;
using static Nexmo.Api.Voice.Call;
using static Nexmo.Api.VoiceHooks;

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
                configFile = "D:\\OneDrive - Nexmo\\Applications\\visual studio apps\\Nexmo PSE Demo\\nexmo-presales\\NexmoPSEDemo\\NexmoPSEDemo\\appsettings.json";
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
                logDirectory = "D:\\OneDrive - Nexmo\\Applications\\visual studio apps\\Nexmo PSE Demo\\nexmo-presales\\NexmoPSEDemo\\Logs\\";
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
        // Verify API
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

        // SMS API
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

        // Messaging API
        public static bool SendMessage(MessagingModel messagingModel, Logger logger, IConfigurationRoot configuration)
        {
            // extract the url and token from the configuration file
            string url = configuration["appSettings:Nexmo.Url.Api"] + "/v0.1/messages";
            string token = configuration["appSettings:Nexmo.Messaging.App.Token"]; // TODO: replace with input from web user???

            if (messagingModel.Type == "WhatsApp")
            {
                url = configuration["appSettings:Nexmo.Url.WA.Sandbox"];
                token = configuration["appSettings:Nexmo.Messaging.WA.Token"];
            }

            // get the json object to pass in the request
            string messageObj = GenerateMessageJson(messagingModel);

            if (messagingModel.Template == "true")
            {
                messageObj = GenerateTemplateMessageJson(messagingModel);
            }

                // start creating the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                logger.Log(JsonConvert.SerializeObject(messageObj, Formatting.Indented));
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

                        return true;
                    }
                    else
                    {
                        logger.Log(Level.Warning, response.StatusCode.ToString());
                        logger.Log(Level.Warning, response.ReasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return false;
        }

        // Disptach API
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

                        return true;
                    }
                    else
                    {
                        logger.Log(Level.Warning, response.StatusCode.ToString());
                        logger.Log(Level.Warning, response.ReasonPhrase);
                    }
                }                
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return false;
        }

        // Voice API
        public static bool MakeVoiceCall(VoiceModel voiceModel, Logger logger, IConfigurationRoot configuration)
        {
            //TODO: Fix the 401 issue. For now testing by generating the jwt token in the terminal
            //string privateKeyString = System.IO.File.ReadAllText(configuration["appSettings:Nexmo.Application.Key"]);
            //byte[] key = Encoding.ASCII.GetBytes(privateKeyString);

            //var tokenData = new byte[64];
            //var rng = RandomNumberGenerator.Create();
            //rng.GetBytes(tokenData);
            //var jwtTokenId = Convert.ToBase64String(tokenData);
            //var iat = (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            //var claims = new[]
            //{
            //    new Claim("iat", iat.ToString()),
            //    new Claim("jti", jwtTokenId),
            //    new Claim("application_id", configuration["appSettings:Nexmo.Application.Id"])
            //};

            //var jwtToken = new JwtSecurityToken(
            //claims: claims,
            //expires: new System.DateTime(2020, 1, 31),
            //signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //);

            //TODO: Fix jwt generation logic. For now, the hard coded token is valid until 31/01/2020.
            //string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            string encodedJwt = configuration["appSettings:Nexmo.Voice.Jwt.Token"];

            // TODO: Implement Nexmo's library code. Currently not working because of RSA issue with private key
            //var client = new Client(creds: new Credentials
            //{
            //    ApiKey = configuration["appSettings:Nexmo.api_key"],
            //    ApiSecret = configuration["appSettings:Nexmo.api_secret"],
            //    ApplicationId = configuration["appSettings:Nexmo.Application.Id"],
            //    ApplicationKey = privateKeyString
            //});

            try
            {
                logger = NexmoLogger.GetLogger("MessagingLogger");
                logger.Open();

                var url = configuration["appSettings:Nexmo.Url.Api"] + "/v1/calls";
                logger.Log(Level.Info, url);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", "Bearer " + encodedJwt);

                var to = new List<CallTo>()
                {
                    new CallTo()
                    {
                        Type = "phone",
                        Number = voiceModel.To
                    }
                };
                var from = new CallFrom()
                {
                    Type = "phone",
                    Number = voiceModel.From
                };
                var eventUrls = new List<string>()
                {
                    "https://nexmopsedemo.azurewebsites.net/api/status"
                };
                List<Ncco> Ncco = new List<Ncco>()
                {
                    new Ncco()
                    {
                        action = voiceModel.Action,
                        text = voiceModel.Text
                    }
                };
                VoiceRootObject requestObject = new VoiceRootObject
                {
                    To = to,
                    From = from,
                    Event_url = eventUrls,
                    Ncco = Ncco
                };

                string jsonRequestContent = JsonConvert.SerializeObject(requestObject);
                request.Content = new StringContent(jsonRequestContent.ToLower(), Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(Level.Info, response.StatusCode);
                        logger.Log(Level.Info, response.RequestMessage);
                        logger.Log(Level.Info, response.Headers);
                        logger.Log(Level.Info, response.Content);

                        return true;
                    }
                    else
                    {
                        logger.Log(Level.Warning, response.StatusCode);
                        logger.Log(Level.Warning, response.ReasonPhrase);
                    }
                }                

                // TODO Implement the Nexmo library once the RSA issue is resolved.
                // MAKE A PHONE CALL
                //Call.CallResponse response = client.Call.Do(new Call.CallCommand
                //{
                //    to = new[]
                //    {
                //        new Call.Endpoint
                //        {
                //            type = "phone",
                //            number = "447843608441"
                //        }
                //    },
                //    from = new Call.Endpoint
                //    {
                //        type = "phone",
                //        number = "33644631466"
                //    },
                //    answer_url = new[]
                //    {
                //        "http://jpchenot.nexmodemo.com/basic-tts-ncco.json"
                //    },
                //    event_url = new[]
                //    {
                //        "https://4bbeea69.ngrok.io/vapi/events"
                //    }
                //});

                //Console.WriteLine(response.conversation_uuid);
                //Console.WriteLine(response.uuid);
                //Console.WriteLine(response.status);
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);

                return false;
            }

            return false;
        }

        public static string AnswerVoiceCall(VoiceInboundObject voiceInboundObject, Logger logger, IConfigurationRoot configuration)
        {
            var request = new HttpRequestMessage();
            string jsonRequestContent = String.Empty;

            try
            {
                List<Ncco> Ncco = new List<Ncco>()
                {
                    new Ncco()
                    {
                        action = "talk",
                        text = "Your sensor in the kitchen has detected some movement. The alarm has been triggered. What would you like to do?"
                    }
                };

                jsonRequestContent = JsonConvert.SerializeObject(Ncco);
                logger.Log("Vapi Inbound Call NCCO: " + jsonRequestContent);

                //var client = GenerateNexmoClient(configuration);
                //CallCommandResponse result = client.Call.BeginTalk(voiceInboundObject.Uuid, new TalkCommand
                //{
                //    text = "",
                //    voice_name = "Kimberly",
                //});
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return jsonRequestContent.ToLower();
        }

        // Number Insight API
        public static NumberInsightBasicResponse BasicNumberInsightRequest(ValidationModel validationModel, IConfigurationRoot configuration)
        {
            var client = GenerateNexmoClient(configuration);

            return client.NumberInsight.RequestBasic(request: new NumberInsight.NumberInsightRequest
            {
                Country = (validationModel.Country == "0") ? "" : validationModel.Country,
                Number = validationModel.Number
            });
        }

        public static NumberInsightStandardResponse StandardNumberInsightRequest(ValidationModel validationModel, IConfigurationRoot configuration)
        {
            var client = GenerateNexmoClient(configuration);

            return client.NumberInsight.RequestStandard(request: new NumberInsight.NumberInsightRequest
            {
                CallerIDName = (validationModel.Cnam == null) ? "" : validationModel.Cnam,
                Country = (validationModel.Country == "0") ? "" : validationModel.Country,
                Number = validationModel.Number
            });
        }

        public static NumberInsightAdvancedResponse AdvancedNumberInsightRequest(ValidationModel validationModel, IConfigurationRoot configuration)
        {
            var client = GenerateNexmoClient(configuration);

            return client.NumberInsight.RequestAdvanced(request: new NumberInsight.NumberInsightRequest
            {
                CallerIDName = (validationModel.Cnam == null) ? "false" : "true",
                Country = (validationModel.Country == "0") ? "" : validationModel.Country,
                Number = validationModel.Number
            });
        }

        // Shared methods to be used by Nexmo APIs methods
        private static Client GenerateNexmoClient(IConfigurationRoot configuration)
        {
            var client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = configuration["appSettings:Nexmo.api_key"],
                ApiSecret = configuration["appSettings:Nexmo.api_secret"],
                AppUserAgent = configuration["appSettings:Nexmo.UserAgent"]
            });

            return client;
        }

        public static BasicObject GenerateBasicObject(NumberInsightBasicResponse response)
        {
            BasicObject responseObject = new BasicObject()
            {
                status = response.Status,
                status_message = response.StatusMessage,
                request_id = response.RequestId,
                international_format_number = response.InternationalFormatNumber,
                national_format_number = response.NationalFormatNumber,
                country_code = response.CountryCode,
                country_code_iso3 = response.CountryCodeIso3,
                country_name = response.CountryName,
                country_prefix = response.CountryPrefix,
            };

            return responseObject;
        }

        public static StandardObject GenerateStandardObject(NumberInsightStandardResponse response)
        {
            CurrentCarrier currentCarrier = new CurrentCarrier()
            {
                network_code = response.CurrentCarrier.NetworkCode,
                name = response.CurrentCarrier.Name,
                country = response.CurrentCarrier.Country,
                network_type = response.CurrentCarrier.NetworkType
            };

            CallerIdentity callerIdentity = new CallerIdentity()
            {
                caller_type = response.CallerType,
                caller_name = response.CallerName,
                first_name = response.FirstName,
                last_name = response.LastName
            };

            StandardObject responseObject = new StandardObject()
            {
                status = response.Status,
                status_message = response.StatusMessage,
                request_id = response.RequestId,
                international_format_number = response.InternationalFormatNumber,
                national_format_number = response.NationalFormatNumber,
                country_code = response.CountryCode,
                country_code_iso3 = response.CountryCodeIso3,
                country_name = response.CountryName,
                country_prefix = response.CountryPrefix,
                current_carrier = currentCarrier,
                caller_identity = callerIdentity
            };

            return responseObject;
        }

        public static AdvancedObject GenerateAdvancedObject(NumberInsightAdvancedResponse response)
        {
            CurrentCarrier currentCarrier = new CurrentCarrier()
            {
                network_code = response.CurrentCarrier.NetworkCode,
                name = response.CurrentCarrier.Name,
                country = response.CurrentCarrier.Country,
                network_type = response.CurrentCarrier.NetworkType
            };

            OriginalCarrier originalCarrier = new OriginalCarrier()
            {
                network_code = response.OriginalCarrier.NetworkCode,
                name = response.OriginalCarrier.Name,
                country = response.OriginalCarrier.Country,
                network_type = response.OriginalCarrier.NetworkType
            };

            Models.Roaming roaming = new Models.Roaming()
            {
                status = response.RoamingInformation.status,
                roaming_country_code = response.RoamingInformation.roaming_country_code,
                roaming_network_code = response.RoamingInformation.roaming_network_code,
                roaming_network_name = response.RoamingInformation.roaming_network_name
            };

            CallerIdentity callerIdentity = new CallerIdentity()
            {
                caller_type = response.CallerType,
                caller_name = response.CallerName,
                first_name = response.FirstName,
                last_name = response.LastName
            };

            AdvancedObject responseObject = new AdvancedObject()
            {
                status = response.Status,
                status_message = response.StatusMessage,
                request_id = response.RequestId,
                international_format_number = response.InternationalFormatNumber,
                national_format_number = response.NationalFormatNumber,
                country_code = response.CountryCode,
                country_code_iso3 = response.CountryCodeIso3,
                country_name = response.CountryName,
                country_prefix = response.CountryPrefix,
                current_carrier = currentCarrier,
                original_carrier = originalCarrier,
                ported = response.PortedStatus,
                roaming = roaming,
                caller_identity = callerIdentity,
                lookup_outcome = response.LookupOutcome.ToString(),
                lookup_outcome_message = response.LookupOutcomeMessage,
                valid_number = response.NumberValidity,
                reachable = response.NumberReachability
            };

            return responseObject;
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

        private static string GenerateTemplateMessageJson(MessagingModel messagingModel)
        {
            string sender = "447418342149";
            if (messagingModel.Type != "WhatsApp")
                sender = messagingModel.Sender;

            var messageJson = new TemplateMessagingObject()
            {
                From = new From() { Type = messagingModel.Type, Number = sender },
                To = new To() { Type = messagingModel.Type, Number = messagingModel.Number },
                Message = new TemplateMessage()
                {
                    Content = new TemplateContent()
                    {
                        Type = "template",
                        Template = new Template()
                        {
                            Name = "whatsapp:hsm:technology:nexmo:simplewelcome",
                            Parameters = new List<Parameter>()
                            {
                                new Parameter(){Default = messagingModel.Brand},
                                new Parameter(){Default = messagingModel.Text}
                            }
                        }
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
