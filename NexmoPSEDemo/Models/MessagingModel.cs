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

    public class MessagingStatus
    {
        public string message_uuid { get; set; }
        public To to { get; set; }
        public From from { get; set; }
        public DateTime timestamp { get; set; }
        public Usage usage { get; set; }
        public string status { get; set; }
    }

    public class Usage
    {
        public string price { get; set; }
        public string currency { get; set; }
    }

    // SMS objects
    public class ChatSmsObject
    {
        public string To { get; set; }
        public string Text { get; set; }
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

    // WhatsApp objects
    public class ChatWAObject
    {
        public string To { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public bool Template { get; set; }
    }

    public class FileChatWAObject
    {
        public string type { get; set; }
        public string path { get; set; }
        public string to { get; set; }
    }

    public class TemplateMessagingObject
    {
        public From From { get; set; }
        public To To { get; set; }
        public TemplateMessage Message { get; set; }
    }

    public class InboundWAObject
    {
        public string message_uuid { get; set; }
        public To to { get; set; }
        public From from { get; set; }
        public DateTime timestamp { get; set; }
        public string direction { get; set; }
        public Message message { get; set; }
    }

    // WhatsApp File objects
    public class File
    {
        public string url { get; set; }
        public string caption { get; set; }
    }

    public class FileContent
    {
        public string type { get; set; }
        public File file { get; set; }
    }

    public class FileMessage
    {
        public FileContent content { get; set; }
    }

    public class FileMessageRequest
    {
        public From from { get; set; }
        public To to { get; set; }
        public FileMessage message { get; set; }
    }
    // WhatsApp Image objects

    public class Image
    {
        public string url { get; set; }
        public string caption { get; set; }
    }

    public class ImageContent
    {
        public string type { get; set; }
        public Image image { get; set; }
    }

    public class ImageMessage
    {
        public ImageContent content { get; set; }
    }

    public class ImageMessageRequest
    {
        public From from { get; set; }
        public To to { get; set; }
        public ImageMessage message { get; set; }
    }

    // WhatsApp audio objects
    public class Audio
    {
        public string url { get; set; }
    }

    public class AudioContent
    {
        public string type { get; set; }
        public Audio audio { get; set; }
    }

    public class AudioMessage
    {
        public AudioContent content { get; set; }
    }

    public class AudioMessageRequest
    {
        public From from { get; set; }
        public To to { get; set; }
        public AudioMessage message { get; set; }
    }
}