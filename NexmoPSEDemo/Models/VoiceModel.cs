using Newtonsoft.Json;
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

    public class Self
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Self self { get; set; }
    }


    public class BasicTTSNcco
    {
        public string action { get; set; }
        public string text { get; set; }
        public string voiceName { get; set; }
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
        public OnAnswer onAnswer { get; set; }
    }

    public class OnAnswer
    {
        public string url { get; set; }
    }

    public class VoiceRootObject
    {
        public List<CallTo> to { get; set; }
        public CallFrom from { get; set; }
        public List<string> event_url { get; set; }
        public List<BasicTTSNcco> ncco { get; set; }
    }

    public class VoiceInputRootObject
    {
        public List<CallTo> to { get; set; }
        public CallFrom from { get; set; }
        public List<string> event_url { get; set; }
        [JsonIgnore]
        public string ncco { get; set; }
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

    public class InFlightCallDetails
    {
        public string uuid { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
        public string rate { get; set; }
        public string price { get; set; }
        public string duration { get; set; }
        public string network { get; set; }
        public string conversation_uuid { get; set; }
        [JsonIgnore]
        public DateTime start_time { get; set; }
        [JsonIgnore]
        public DateTime end_time { get; set; }
        public To to { get; set; }
        public From from { get; set; }
        public Links _links { get; set; }
    }

    public class CallStatus
    {
        public DateTime end_time { get; set; }
        public string uuid { get; set; }
        public string network { get; set; }
        public string duration { get; set; }
        public DateTime start_time { get; set; }
        public string rate { get; set; }
        public string price { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string conversation_uuid { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class CallDetails
    {
        public string uuid { get; set; }
        public string status { get; set; }
        public string direction { get; set; }
        public string rate { get; set; }
        public string price { get; set; }
        public string duration { get; set; }
        public string network { get; set; }
        public string conversation_uuid { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public To to { get; set; }
        public From from { get; set; }
        public Links _links { get; set; }
    }

    public class VoiceAssitantModel
    {
        public string language { get; set; }
    }

    public class Speech
    {
        public string enable { get; set; }
        public List<string> context { get; set; }
        public string language { get; set; }
        public List<string> uuid { get; set; }
        public string priority { get; set; }
        public int endOnSilence { get; set; }
    }

    public class Dtmf
    {
        public bool enable { get; set; }
        public string priority { get; set; }
        public bool submitOnHash { get; set; }
        public int maxDigits { get; set; }
        public int timeOut { get; set; }
    }

    public class AsrInputNcco
    {
        public string action { get; set; }
        public Speech speech { get; set; }
        public Dtmf dtmf { get; set; }
        public List<string> eventUrl { get; set; }
        public string eventMethod { get; set; }
    }

    public class CallerDetails
    {
        public string name { get; set; }
        public string number { get; set; }
    }
}
