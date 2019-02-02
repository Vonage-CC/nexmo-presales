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
    }

    public class MessagingObject
    {
        public From From { get; set; }
        public To To { get; set; }
        public Message Message { get; set; }
    }
}