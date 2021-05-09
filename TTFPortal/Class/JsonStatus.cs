using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TTFPortal.Class
{
    public class JsonStatus
    {
        public int code { get; set; }
        public string text { get; set; }
        public string description { get; set; }
        public object data { get; set; }
        public string error { get; set; }
    }
}