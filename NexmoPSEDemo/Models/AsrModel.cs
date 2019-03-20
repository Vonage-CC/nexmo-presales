using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexmoPSEDemo.Models
{
    public class Result
    {
        public string confidence { get; set; }
        public string text { get; set; }
    }

    public class AsrSpeech
    {
        public List<Result> results { get; set; }
    }

    public class AsrDtmf
    {
        public string digits { get; set; }
        public bool timed_out { get; set; }
    }

    public class AsrInputObject
    {
        public AsrSpeech speech { get; set; }
        public AsrDtmf dtmf { get; set; }
        public string uuid { get; set; }
        public string conversation_uuid { get; set; }
        public DateTime timestamp { get; set; }
    }
}
