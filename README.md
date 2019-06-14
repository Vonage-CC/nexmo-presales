# Nexmo APIs Pre-Sales Demo
Getting Started with Nexmo Pre-sales demo built with C#.
This is a list of prerequirements to run the demo application within Visual Studio.

1. Create an appSettings.json file in your project's root. Populate it with the following json:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "appSettings": {
    "Nexmo.api_key": "your API key",
    "Nexmo.api_secret": "your API secret",
    "Nexmo.api_key.sub": "your secondary API key",
    "Nexmo.api_secret.sub": "your secondary API secret",
    "Nexmo.Application.Id": "if using the Voice API",
    "Nexmo.Application.Number.From.FR": "for using the Messaging API and VAPI",
    "Nexmo.Application.Number.From.UK": "for using the Messaging API and VAPI",
    "Nexmo.Messaging.WA.Token": "WhatsApp sandbox token",
    "Nexmo.Messaging.WA.Sender": "the WhatsApp sandbox number",
    "Nexmo.Url.Api": "https://api.nexmo.com",
    "Nexmo.Url.Rest": "https://rest.nexmo.com",
    "Nexmo.Url.WA.Sandbox": "https://sandbox.nexmodemo.com/v0.1/messages/",
    "Nexmo.UserAgent": "your app name/1.0",
    "Nexmo.Voice.Url.Event": "your Event / Status call back URL",
    "Nexmo.Voice.Url.Input": "your input call back URL",
    "Logs.Path": "the path to your logs folder",
    "OT.Api.Key": "OpenTok project API key",
    "OT.Api.Secret": "OpenTok project API secret",
    "OT.Session.Monitoring.Url": "your OpenTok monitoring callback URL"
  },
  "ConnectionStrings": {
    "AzureStorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=your-Azure-storage-account-name;AccountKey=your Azure storage account key;EndpointSuffix=core.windows.net"
  }
}
```

2. Update the Storage class in Common/Common.cs
In the GetCloudBlobContainer() method, change this line CloudBlobContainer container = blobClient.GetContainerReference("vapi-connect-container"); and replace vapi-connect-container with the name you would like to use

3. Update the Configuration class in Common/Common.cs
In the GetConfigFile() method, change the path of the configFile variable depending on where your solution is located on your machine.

4. For the Voice API, you must store a copy of your generated key file in the root folder of your project. The file must be called 'private.key'.

5. For the Messaging API, particularly for the WhatsApp sandbox, you must fill in the parameters Nexmo.Messaging.WA.Token and  Nexmo.Messaging.WA.Sender. 

6. For the OpenTok demo, you must create a project in your OpenTok dashboard and you must fill in the parameters prefixed with OT.

7. For the Verify API, you must specify a secondary API key and secret. This API key, must have the Pay-Per-Request pricing model enabled.
