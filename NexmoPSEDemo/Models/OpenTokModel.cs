using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenTokSDK;

namespace NexmoPSEDemo.Models
{
    public class ClientSession
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ArchiveMode { get; set; }
        public string Id { get; set; }
        public string Location { get; set; }
        public string MediaMode { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
    }

    public class SessionName
    {
        public string Name { get; set; }
    }

    public class Connection
    {
        public string id { get; set; }
        public long createdAt { get; set; }
        public string data { get; set; }
    }

    public class Stream
    {
        public string id { get; set; }
        public Connection connection { get; set; }
        public long createdAt { get; set; }
        public string name { get; set; }
        public string videoType { get; set; }
    }

    public class SessionConnection
    {
        public string sessionId { get; set; }
        public string projectId { get; set; }
        public string @event { get; set; }
        public string reason { get; set; }
        public long timestamp { get; set; }
        public Connection connection { get; set; }
    }

    public class SessionStream
    {
        public string sessionId { get; set; }
        public string projectId { get; set; }
        public string @event { get; set; }
        public string reason { get; set; }
        public long timestamp { get; set; }
        public Stream stream { get; set; }
    }

    public class ArchiveRequest
    {
        public string sessionId { get; set; }
    }

    public class StopArchiveRequest
    {
        public string archiveId { get; set; }
    }

    public class Archive
    {
        public string sessionId { get; set; }
        public string name { get; set; }
        public bool hasVideo { get; set; }
        public bool hasAudio { get; set; }
        public OutputMode outputMode { get; set; }
        public string resolution { get; set; }
    }
}