using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class LogIn2FaRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string LoginToken { get; set; } = string.Empty;
    }
}