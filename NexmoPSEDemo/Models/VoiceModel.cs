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
        public string type { get; set; }
        public string number { get; set; }
    }

    public class CallFrom
    {
        public string type { get; set; }
        public string number { get; set; }
    }

    public class BasicTTSNcco
    {
        public string action { get; set; }
        public string text { get; set; }
    }

    public class BargeInTTSNcco
    {
        public string action { get; set; }
        public string text { get; set; }
        public bool bargeIn { get; set; }
    }

    public class InputTTSNcco
    {
        public string action { get; set; }
        public List<string> eventUrl { get; set; }
    }

    public class VoiceRecipient
    {
        public string recipient { get; set; }
    }

    public class VoiceEndpoint
    {
        public string type { get; set; }
        public string number { get; set; }
        public string dtmfAnswer { get; set; }
    }

    public class VoiceRootObject
    {
        public List<CallTo> to { get; set; }
        public CallFrom from { get; set; }
        public List<string> event_url { get; set; }
        public List<BasicTTSNcco> ncco { get; set; }
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
        public bool Timed_out { get; set; }
        public string Dtmf { get; set; }
        public DateTime Timestamp { get; set; }
        public string Uuid { get; set; }
        public string Conversation_uuid { get; set; }
    }

    public class VoiceConnectObject
    {
        public string action { get; set; }
        public List<string> eventUrl { get; set; }
        public string timeout { get; set; }
        public string from { get; set; }
        public List<VoiceEndpoint> endpoint { get; set; }
    }
}
