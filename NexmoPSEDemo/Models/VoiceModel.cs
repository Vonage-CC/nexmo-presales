using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexmoPSEDemo.Models
{
    public class VoiceModel
    {
    }

    public class CallTo
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class CallFrom
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class VoiceRootObject
    {
        public List<CallTo> To { get; set; }
        public CallFrom From { get; set; }
        //public List<string> Answer_url { get; set; }
        public string Ncco { get; set; }
        public List<string> Event_url { get; set; }
    }
}
