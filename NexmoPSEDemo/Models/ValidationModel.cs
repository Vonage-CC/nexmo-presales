
namespace NexmoPSEDemo.Models
{
    public class ValidationModel
    {
        public string Cnam { get; set; }
        public string Number { get; set; }
        public string Country { get; set; }
        public string Version { get; set; }
    }

    public class CurrentCarrier
    {
        public string network_code { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string network_type { get; set; }
    }

    public class OriginalCarrier
    {
        public string network_code { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string network_type { get; set; }
    }

    public class Roaming
    {
        public string status { get; set; }
        public string roaming_country_code { get; set; }
        public string roaming_network_code { get; set; }
        public string roaming_network_name { get; set; }
    }

    public class CallerIdentity
    {
        public string caller_type { get; set; }
        public string caller_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }

    public class Ip
    {
        public string address { get; set; }
        public string ip_match_level { get; set; }
        public string ip_country { get; set; }
        public string ip_city { get; set; }
    }

    public class BasicObject
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public string request_id { get; set; }
        public string international_format_number { get; set; }
        public string national_format_number { get; set; }
        public string country_code { get; set; }
        public string country_code_iso3 { get; set; }
        public string country_name { get; set; }
        public string country_prefix { get; set; }
    }

    public class StandardObject
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public string request_id { get; set; }
        public string international_format_number { get; set; }
        public string national_format_number { get; set; }
        public string country_code { get; set; }
        public string country_code_iso3 { get; set; }
        public string country_name { get; set; }
        public string country_prefix { get; set; }
        public CurrentCarrier current_carrier { get; set; }
        public CallerIdentity caller_identity { get; set; }
    }

    public class AdvancedObject
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public string request_id { get; set; }
        public string international_format_number { get; set; }
        public string national_format_number { get; set; }
        public string country_code { get; set; }
        public string country_code_iso3 { get; set; }
        public string country_name { get; set; }
        public string country_prefix { get; set; }
        public CurrentCarrier current_carrier { get; set; }
        public OriginalCarrier original_carrier { get; set; }
        public string ported { get; set; }
        public Roaming roaming { get; set; }
        public CallerIdentity caller_identity { get; set; }
        public string lookup_outcome { get; set; }
        public string lookup_outcome_message { get; set; }
        public string valid_number { get; set; }
        public string reachable { get; set; }
        //public Ip ip { get; set; }
        //public string ip_warnings { get; set; }
    }
}
