using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.Email
{
    public class EmailDataDto
    {
        public string EmailTo { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateData { get; set; } = [];
    }
}