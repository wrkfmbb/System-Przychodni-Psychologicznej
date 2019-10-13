using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace auth.Models
{

    public class Email
    {
        public string To { get; set; }
        public string From { get; set; }
        public string DisplayNameFrom { get; set;  }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

}