using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenTokServer.Models
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
}