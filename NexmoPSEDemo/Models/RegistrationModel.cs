using System;
namespace NexmoPSEDemo.Models
{
    public class RegistrationModel
    {
        public string RequestId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Feedback { get; set; }
        public string PinCode { get; set; }
        public string PinCodeCheck { get; set; }
        public string Recipient { get; set; }
        public string Model { get; set; }
        public string Workflow { get; set; }
    }

    public class VerifyRequest
    {
        public string number { get; set; }
        public string country { get; set; }
        public string brand { get; set; }
        public string sender_id { get; set; }
        public string code_length { get; set; }
        public string lg { get; set; }
        public string require_type { get; set; }
        public string pin_expiry { get; set; }
        public string next_event_wait { get; set; }
        public string pin_code { get; set; }
    }

    public class VerifyResponse
    {
        public string request_id { get; set; }
        public string status { get; set; }
        public string error_text { get; set; }
    }
}
