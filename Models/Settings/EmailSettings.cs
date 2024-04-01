using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Models.Settings
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Password { get; set; }
    }
}