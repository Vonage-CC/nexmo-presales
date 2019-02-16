using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexmoPSEDemo.Models
{
    public class FMTo
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class FMFrom
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class FMContent
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class FMMessage
    {
        public Content content { get; set; }
    }

    public class FMRootObject
    {
        public string message_uuid { get; set; }
        public To to { get; set; }
        public From from { get; set; }
        public DateTime timestamp { get; set; }
        public string direction { get; set; }
        public Message message { get; set; }
    }
}
