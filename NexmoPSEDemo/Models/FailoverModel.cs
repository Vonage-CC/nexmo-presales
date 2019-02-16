using System;
using System.Collections.Generic;

namespace NexmoPSEDemo.Models
{
    public class FailoverModel
    {
        public string RequestId { get; set; }        
    }

    public class From
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class To
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class Parameter
    {
        public string Default { get; set; }
    }

    public class Template
    {
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class TemplateContent
    {
        public string Type { get; set; }
        public Template Template { get; set; }
    }

    public class Content
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }

    public class Message
    {
        public Content Content { get; set; }
    }

    public class TemplateMessage
    {
        public TemplateContent Content { get; set; }
    }

    public class Failover
    {
        public int Expiry_time { get; set; }
        public string Condition_status { get; set; }
    }

    public class Workflow
    {
        public From From { get; set; }
        public To To { get; set; }
        public Message Message { get; set; }
        public Failover Failover { get; set; }
        public From From2 { get; set; }
        public To To2 { get; set; }
        public Message Message2 { get; set; }
    }

    public class DispatchRootObject
    {
        public string Template { get; set; }
        public Workflow Workflow { get; set; }
    }
}