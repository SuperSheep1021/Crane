using LeanCloud;
using System;
using System.Collections.Generic;

namespace web.Models
{
    //public class ClientMessageModel
    //{
    //    public string Id { get; set; } = Guid.NewGuid().ToString();
    //    public string SenderId { get; set; }
    //    public string Content { get; set; }
    //    public string Type { get; set; } = "text";
    //    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    //    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    //}
    public class LCMessage : AVObject
    {
        public LCMessage() : base("AllMessage") { }

        public string SenderId
        {
            get { return GetProperty<string>("SenderId"); }
            set { SetProperty<string>(value, "SenderId"); }
        }

        public string Content
        {
            get { return GetProperty<string>("Content"); }
            set { SetProperty<string>(value, "Content"); }
        }

        public string Type
        {
            get { return GetProperty<string>("Type"); }
            set { SetProperty<string>(value, "Type"); }
        }

        public bool IsProcessed
        {
            get { return GetProperty<bool>("isProcessed"); }
            set { SetProperty<bool>(value, "isProcessed"); }
        }
    }

}
