using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.Email
{
    public class EmailDataDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string EmailTo { get; set; } = string.Empty;
        [Required]
        [MaxLength(988)]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        [Required]
        public Dictionary<string, string> TemplateData { get; set; } = [];
    }
}