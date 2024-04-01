using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Models.Settings
{
    public class EmailSettings
    {
        [Required]
        public string SmtpServer { get; set; } = string.Empty;
        [Required]
        public int Port { get; set; }
        [Required]
        public bool UseSSL { get; set; }
        [Required]
        public string SenderEmail { get; set; } = string.Empty;
        [Required]
        public string SenderName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}