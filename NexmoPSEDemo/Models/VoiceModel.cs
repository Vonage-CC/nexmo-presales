using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexmoPSEDemo.Models
{
    public class VoiceModel
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Action { get; set; }
        public string Text { get; set; }
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

    public class BasicTTSNcco
    {
        public string Action { get; set; }
        public string Text { get; set; }
    }

    public class BargeInTTSNcco
    {
        public string Action { get; set; }
        public string Text { get; set; }
        public bool BargeIn { get; set; }
    }

    public class InputTTSNcco
    {
        public string Action { get; set; }
        public List<string> EventUrl { get; set; }
    }

    public class InputTTSNccoWithBargeIn
    {
        public BargeInTTSNcco TTS { get; set; }
        public InputTTSNcco Input { get; set; }
    }

    public class VoiceRootObject
    {
        public List<CallTo> To { get; set; }
        public CallFrom From { get; set; }
        public List<string> Event_url { get; set; }
        public List<BasicTTSNcco> Ncco { get; set; }
    }

    public class VoiceInboundObject
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Uuid { get; set; }
        public string Conversation_uuid { get; set; }
    }

    public class VoiceInputObject
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Uuid { get; set; }
        public string Conversation_uuid { get; set; }
    }
}
