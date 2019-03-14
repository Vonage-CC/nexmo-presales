using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NSpring.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace OpenTokServer.Common
{
    public static class NexmoLogger
    {
        public static Logger GetLogger(string loggerName)
        {
            string logDirectory;
            // Make sure the directory to write the log files to exists
#if DEBUG
            // use this for Mac OS
            logDirectory = "/Volumes/GoogleDrive/My Drive/Documents/nexmo/visual studio apps/Nexmo PSE Demo/nexmo-presales/OpenTokServer/Logs/";
            // use this for PC
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                logDirectory = ConfigurationManager.AppSettings["Logs.Path"];
            }
#else
            logDirectory = "../../../LogFiles/Application/";
#endif
            // Make sure the directory to write the log files to exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create and configure the file logger
            var fileLogger = Logger.CreateFileLogger(logDirectory + "opentok_log-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ".txt");
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
            //var connString = configuration["ConnectionStrings:AzureStorageConnectionString"];
            var storageAccount = CloudStorageAccount.Parse("");
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
            catch (Exception e)
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
                    if (blob.Name == blockBlob)
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
            //IConfigurationRoot configuration = Configuration.GetConfigFile();

            //// Generate a token ID
            //var tokenData = new byte[64];
            //var rng = RandomNumberGenerator.Create();
            //rng.GetBytes(tokenData);
            //var jwtTokenId = Convert.ToBase64String(tokenData);

            //var payload = new Dictionary<string, object>
            //{
            //    { "iat", (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds },
            //    { "application_id", configuration["appSettings:Nexmo.Application.Id"] },
            //    { "jti", jwtTokenId }
            //};

            //string privateKeyString = File.ReadAllText("private.key");
            //var rsa = PemParse.DecodePEMKey(privateKeyString);
            //var jwtToken = JWT.Encode(payload, rsa, JwsAlgorithm.RS256);

            //return jwtToken;
            return "";
        }
    }
}
