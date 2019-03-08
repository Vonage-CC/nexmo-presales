using Jose;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Nexmo.Api;
using Nexmo.Api.Request;
using Nexmo.Api.Voice;
using NexmoPSEDemo.Models;
using NSpring.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Nexmo.Api.NumberInsight;
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
                configFile = "D:\\Documents\\My Web Sites\\nexmo-presales\\NexmoPSEDemo\\appsettings.json";
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
            IConfigurationRoot configuration = Configuration.GetConfigFile();
            string logDirectory;
            // Make sure the directory to write the log files to exists
#if DEBUG
            // use this for Mac OS
            logDirectory = "/Volumes/GoogleDrive/My Drive/Documents/nexmo/visual studio apps/Nexmo PSE Demo/nexmo-presales/NexmoPSEDemo/Logs/";
            // use this for PC
            if(Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                logDirectory = configuration["appSettings:Logs.Path"];
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

    public static class Storage
    {
        public static CloudBlobContainer GetCloudBlobContainer()
        {
            var configuration = Configuration.GetConfigFile();
            var connString = configuration["ConnectionStrings:AzureStorageConnectionString"];
            var storageAccount = CloudStorageAccount.Parse(connString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("vapi-connect-container");
            return container;
        }

        public static async Task<bool> UploadBlobAsync(CloudBlobContainer container, Logger logger, string recipient, string blockBlob)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(blockBlob);
            try
            {
                await blob.UploadTextAsync("{\"recipient\": \"" + recipient + "\"}");
                logger.Log(Level.Exception, "Blob upload completed successfully.");
                return true;
            }
            catch(Exception e)
            {
                logger.Log(Level.Exception, "Blob upload did not succeed: " + e.Message);
            }

            return false;
        }

        public static string GetBlob(string blockBlob)
        {
            CloudBlobContainer container = GetCloudBlobContainer();
            string blobValue = string.Empty;
            var blobs = container.ListBlobsSegmentedAsync(new BlobContinuationToken() { NextMarker = "" }).Result;
            foreach (IListBlobItem item in blobs.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    if(blob.Name == blockBlob)
                    {
                        blobValue = blob.DownloadTextAsync().Result;
                    }
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob blob = (CloudPageBlob)item;
                    // TODO: Implement logic
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory dir = (CloudBlobDirectory)item;
                    // TODO: Implement logic
                }
            }

            return blobValue;
        }
    }

    public static class Security
    {
        public static string GenerateJwtToken()
        {
            // Load the configuration file
            IConfigurationRoot configuration = Configuration.GetConfigFile();

            // Generate a token ID
            var tokenData = new byte[64];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenData);
            var jwtTokenId = Convert.ToBase64String(tokenData);

            var payload = new Dictionary<string, object>
            {
                { "iat", (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds },
                { "application_id", configuration["appSettings:Nexmo.Application.Id"] },
                { "jti", jwtTokenId }
            };

            string privateKeyString = File.ReadAllText(configuration["appSettings:Nexmo.Application.Key"]);
            var rsa = PemParse.DecodePEMKey(privateKeyString);
            var jwtToken = JWT.Encode(payload, rsa, JwsAlgorithm.RS256);

            return jwtToken;
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
                        logger.Log(Level.Info, response.Content.ReadAsStringAsync().Result);

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
        public static bool MakeBasicTTSCall(VoiceModel voiceModel, Logger logger, IConfigurationRoot configuration)
        {
            string encodedJwt = Security.GenerateJwtToken();

            // TODO: Implement Nexmo's library code. Currently not working because of RSA issue with private key
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
                        type = "phone",
                        number = voiceModel.To
                    }
                };
                var from = new CallFrom()
                {
                    type = "phone",
                    number = voiceModel.From
                };
                var eventUrls = new List<string>()
                {
                    configuration["appSettings:Nexmo.Voice.Url.Event"]
                };
                List<BasicTTSNcco> Ncco = new List<BasicTTSNcco>()
                {
                    new BasicTTSNcco()
                    {
                        action = voiceModel.Action,
                        text = voiceModel.Text
                    }
                };
                VoiceRootObject requestObject = new VoiceRootObject
                {
                    to = to,
                    from = from,
                    event_url = eventUrls,
                    ncco = Ncco
                };

                string jsonRequestContent = JsonConvert.SerializeObject(requestObject, Formatting.Indented);
                request.Content = new StringContent(jsonRequestContent, Encoding.UTF8, "application/json");

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
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);

                return false;
            }

            return false;
        }

        public static bool MakeAlertTTSCall(VoiceModel voiceModel, Logger logger, IConfigurationRoot configuration)
        {
            string encodedJwt = Security.GenerateJwtToken();

            // TODO: Implement Nexmo's library code. Currently not working because of RSA issue with private key
            try
            {
                logger = NexmoLogger.GetLogger("MessagingLogger");
                logger.Open();

                var url = configuration["appSettings:Nexmo.Url.Api"] + "/v1/calls";
                logger.Log(Level.Info, url);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", "Bearer " + encodedJwt);

                // Add the recipient to the request json
                string ttsIvrCallRequestJson = "{\"to\":";
                var to = new List<CallTo>()
                {
                    new CallTo()
                    {
                        type = "phone",
                        number = voiceModel.To
                    }
                };
                ttsIvrCallRequestJson += JsonConvert.SerializeObject(to, Formatting.Indented);

                // Add the sender to the request json
                ttsIvrCallRequestJson += ",\"from\":";
                var from = new CallFrom()
                {
                    type = "phone",
                    number = voiceModel.From
                };
                ttsIvrCallRequestJson += JsonConvert.SerializeObject(from, Formatting.Indented);

                // Add the event Urls to the request json
                ttsIvrCallRequestJson += ",\"event_url\":";
                var eventUrls = new List<string>()
                {
                    configuration["appSettings:Nexmo.Voice.Url.Event"]
                };
                ttsIvrCallRequestJson += JsonConvert.SerializeObject(eventUrls, Formatting.Indented);

                // Add the ncco to the request json
                ttsIvrCallRequestJson += ",\"ncco\":";
                ttsIvrCallRequestJson += AnswerVoiceCall(logger, configuration);

                // Close the NCCO json string
                ttsIvrCallRequestJson += "}";

                logger.Log("Make TTS Call request json: " + ttsIvrCallRequestJson);
                request.Content = new StringContent(ttsIvrCallRequestJson, Encoding.UTF8, "application/json");

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
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);

                return false;
            }

            return false;
        }

        public static string AnswerVoiceCall(Logger logger, IConfigurationRoot configuration)
        {
            var request = new HttpRequestMessage();
            string jsonRequestContent = String.Empty;

            try
            {
                // Open the NCCO json string
                string ivrInputNcco = "[";

                // Add the talk action to the NCCO
                var bargeInAction = new BargeInTTSNcco()
                {
                    action = "talk",
                    text = "Your sensor in the kitchen has detected some movement. The alarm has been triggered. To call your emergency contact, please press 1. Or to acknowledge receipt of this alert, please press 2.",
                    bargeIn = true
                };
                ivrInputNcco += JsonConvert.SerializeObject(bargeInAction, Formatting.Indented);

                // Add the separator between the various actions
                ivrInputNcco += ",";

                // Add the input action to the NCCO
                var inputAction = new InputTTSNcco()
                {
                    action = "input",
                    eventUrl = new List<string>() { configuration["appSettings:Nexmo.Voice.Url.Input"] }
                };
                ivrInputNcco += JsonConvert.SerializeObject(inputAction, Formatting.Indented);

                // Close the NCCO json string
                ivrInputNcco += "]";

                jsonRequestContent = ivrInputNcco;
                logger.Log("Vapi Inbound Call NCCO: " + jsonRequestContent);
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return jsonRequestContent;
        }

        public static string AnswerVoiceCallInput(VoiceInputObject voiceInputObject, Logger logger, IConfigurationRoot configuration)
        {
            var request = new HttpRequestMessage();
            string jsonRequestContent = String.Empty;

            try
            {
                var inFlightCallDetails = GetInFlightCallDetails(voiceInputObject.Uuid, logger, configuration);
                logger.Log("Retrieved in flight call details. Deciding what action to take based on user input: " + voiceInputObject.Dtmf);

                switch (voiceInputObject.Dtmf)
                {
                    case "1":
                        // Connect the caller to their preferred number
                        logger.Log("User input: 1. Triggering connect action.");
                        jsonRequestContent = GenerateConnectNcco(configuration);
                        logger.Log("Vapi Inbound Call NCCO: " + jsonRequestContent);

                        break;
                    case "2":
                        // Confirm acknowledgement and send confirmation message
                        logger.Log("User input: 2. Triggering WhatsApp message.");
                        logger.Log("Starting sending SMS message in response to ackowledgement to alarm alert.");
                        jsonRequestContent = GenerateAcknowledgementConfirmationNccoAndMessage(inFlightCallDetails, logger, configuration);

                        break;
                    default:
                        logger.Log("User input: " + voiceInputObject.Dtmf + ". Triggering basic invalid input basic action.");
                        var invalidInputAction = new List<BasicTTSNcco>()
                        {
                            new BasicTTSNcco()
                            {
                                action = "talk",
                                text = "We are sorry but your input was invalid. Please call again. Good bye."
                            }
                        };

                        jsonRequestContent = JsonConvert.SerializeObject(invalidInputAction, Formatting.Indented);

                        break;
                }

                logger.Log("Vapi Input Call Reply NCCO: " + jsonRequestContent);
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return jsonRequestContent;
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

            var parameters = new List<Parameter>();
            parameters.Add(new Parameter()
            {
                Default = messagingModel.Brand
            });
            if (!string.IsNullOrEmpty(messagingModel.Text))
            {
                parameters.Add(new Parameter()
                {
                    Default = messagingModel.Text
                });
            }

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
                            Name = messagingModel.TemplateName,
                            Parameters = parameters
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

        private static InFlightCallDetails GetInFlightCallDetails(string uuid, Logger logger, IConfigurationRoot configuration)
        {
            //TODO: Fix jwt generation logic. For now, the hard coded token is valid until 31/01/2020.
            string encodedJwt = configuration["appSettings:Nexmo.Voice.Jwt.Token"];
            InFlightCallDetails inFlightCallDetails = new InFlightCallDetails();

            // TODO: Implement Nexmo's library code. Currently not working because of RSA issue with private key
            try
            {
                var url = configuration["appSettings:Nexmo.Url.Api"] + "/v1/calls/" + uuid;
                logger.Log(Level.Info, url);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Bearer " + encodedJwt);

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
                    var rawInFlightCallDetails = response.Content.ReadAsStringAsync().Result;
                    logger.Log("Call Details Raw Response: " + rawInFlightCallDetails);
                    inFlightCallDetails = JsonConvert.DeserializeObject<InFlightCallDetails>(rawInFlightCallDetails, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    logger.Log("Returning In Flight Call Details json: " + JsonConvert.SerializeObject(inFlightCallDetails, Formatting.Indented));

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(Level.Info, "In Flight Call Details response status code: " + response.StatusCode);
                        logger.Log(Level.Info, "In Flight Call Details response RequestMessage: " + response.RequestMessage);
                        logger.Log(Level.Info, "In Flight Call Details response headers: " + response.Headers);
                        logger.Log(Level.Info, "In Flight Call Details response content: " + response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        logger.Log(Level.Warning, "In Flight Call Details response status code: " + response.StatusCode);
                        logger.Log(Level.Warning, "In Flight Call Details response error description: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return inFlightCallDetails;
        }

        private static CallDetails GetCallDetails(string uuid, Logger logger, IConfigurationRoot configuration)
        {
            //TODO: Fix jwt generation logic. For now, the hard coded token is valid until 31/01/2020.
            string encodedJwt = configuration["appSettings:Nexmo.Voice.Jwt.Token"];
            CallDetails callDetails = new CallDetails();

            // TODO: Implement Nexmo's library code. Currently not working because of RSA issue with private key
            try
            {
                logger = NexmoLogger.GetLogger("CallDetailsLogger");
                logger.Open();

                var url = configuration["appSettings:Nexmo.Url.Api"] + "/v1/calls/" + uuid;
                logger.Log(Level.Info, url);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Bearer " + encodedJwt);

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseContentRead).Result;
                    logger.Log("Call Details Raw Response: " + response.Content.ReadAsStringAsync().Result);
                    callDetails = JsonConvert.DeserializeObject<CallDetails>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Log(Level.Info, "Call Details response status code: " + response.StatusCode);
                        logger.Log(Level.Info, "Call Details response RequestMessage: " + response.RequestMessage);
                        logger.Log(Level.Info, "Call Details response headers: " + response.Headers);
                        logger.Log(Level.Info, "Call Details response content: " + response.Content);
                    }
                    else
                    {
                        logger.Log(Level.Warning, "Call Details response status code: " + response.StatusCode);
                        logger.Log(Level.Warning, "Call Details response error description: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(Level.Exception, e.Message);
                logger.Log(Level.Exception, e.StackTrace);
            }

            return callDetails;
        }

        private static string GenerateAcknowledgementConfirmationNccoAndMessage(InFlightCallDetails callDetails, Logger logger, IConfigurationRoot configuration)
        {
            var ncco = string.Empty;

            // Send SMS message to confirm reception of acknowledgement
            var message = new MessagingModel()
            {
                Number = callDetails.to.Number,
                Sender = callDetails.from.Number,
                Text = "We have called you because of an alarm has been triggered in your house. You have confirmed reception of our alert. For any further help please reply to this message. The Alaram Systems Ltd team.",
            };

            logger.Log("Sending alarm alert SMS message with NCCO: " + JsonConvert.SerializeObject(message, Formatting.Indented));

            var smsResults = NexmoApi.SendSMS(message, configuration, "");
            foreach (SMS.SMSResponseDetail responseDetail in smsResults.messages)
            {
                string messageDetails = "SMS sent successfully with messageId: " + responseDetail.message_id;
                messageDetails += " \n to: " + responseDetail.to;
                messageDetails += " \n at price: " + responseDetail.message_price;
                messageDetails += " \n with status: " + responseDetail.status;
                logger.Log(messageDetails);
            }

            if(smsResults.messages[0].status == "0")
            {
                // Send an NCCO back to confirm the acknowledgement has been received
                var confirmAction = new List<BasicTTSNcco>()
                            {
                                new BasicTTSNcco()
                                {
                                    action = "talk",
                                    text = "Your input has been registered. An SMS confirmation message has been sent. Thank you. Good bye."
                                }
                            };

                ncco = JsonConvert.SerializeObject(confirmAction, Formatting.Indented);
                logger.Log("SMS message successful - Generated alert acknowledgement confirmation NCCO: " + ncco);

                return ncco;
            }
            else
            {
                // Send an NCCO back to confirm the acknowledgement has been received and inform that the confirmation could not be sent via message.
                var confirmAction = new List<BasicTTSNcco>()
                            {
                                new BasicTTSNcco()
                                {
                                    action = "talk",
                                    text = "Your input has been registered. However we could not send an SMS confirmation message. Thank you. Good bye."
                                }
                            };

                ncco = JsonConvert.SerializeObject(confirmAction, Formatting.Indented);
                logger.Log("SMS message failed - Generated alert acknowledgement confirmation NCCO: " + ncco);

                return ncco;
            }
        }

        private static string GenerateConnectNcco(IConfigurationRoot configuration)
        {
            // Open the NCCO json string
            string ivrInputNcco = "[";

            // Add the talk action to the NCCO
            var basicAction = new BasicTTSNcco()
            {
                action = "talk",
                text = "Please wait while we connect you"
            };
            ivrInputNcco += JsonConvert.SerializeObject(basicAction, Formatting.Indented);

            // Add the separator between the various actions
            ivrInputNcco += ",";

            // Get the recipient's phone number to use in the endpoint
            string recipientBlob = Storage.GetBlob("alarmAlert");
            VoiceRecipient voiceRecipient = JsonConvert.DeserializeObject<VoiceRecipient>(recipientBlob);

            // Add the input action to the NCCO
            var endpoint = new List<VoiceEndpoint>()
            {
                new VoiceEndpoint(){
                    type = "phone",
                    number = voiceRecipient.recipient,
                    dtmfAnswer = "1"
                }
            };
            var voiceConnectAction = new VoiceConnectObject()
            {
                action = "connect",
                eventUrl = new List<string>() { "https://nexmopsedemo.azurewebsites.net/vapi/status" },
                timeout = "45",
                from = configuration["appSettings:Nexmo.Application.Number.From.UK"],
                endpoint = endpoint
            };
            ivrInputNcco += JsonConvert.SerializeObject(voiceConnectAction, Formatting.Indented);

            // Close the NCCO json string
            ivrInputNcco += "]";

            return ivrInputNcco;            
        }
    }
}
