using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.Email
{
    public class EmailDataDto
    {
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public string TemplateName { get; set; }
        public Dictionary<string, string> TemplateData { get; set; }
    }
}