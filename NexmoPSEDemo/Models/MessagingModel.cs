using Newtonsoft.Json;
using System;

namespace NexmoPSEDemo.Models
{
    public class MessagingModel
    {
        public string Sender { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string ContentType { get; set; }
        public string Text { get; set; }
        public string RequestId { get; set; }
        public string Template { get; set; }
        public string TemplateName { get; set; }
        public string Brand { get; set; }
    }

    public class MessagingObject
    {
        public From From { get; set; }
        public To To { get; set; }
        public Message Message { get; set; }
    }

    public class TemplateMessagingObject
    {
        public From From { get; set; }
        public To To { get; set; }
        public TemplateMessage Message { get; set; }
    }

    public class InboundSmsObject
    {
        public string msisdn { get; set; }
        public string to { get; set; }
        public string messageId { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public string keyword { get; set; }
        [JsonProperty(PropertyName = "message-timestamp")]
        public string message_timestamp { get; set; }
    }
}