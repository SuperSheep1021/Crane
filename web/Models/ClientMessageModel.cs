using LeanCloud;
using System;
using System.Collections.Generic;

namespace web.Models
{
    public class ClientMessageModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderId { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } = "text";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }


}
