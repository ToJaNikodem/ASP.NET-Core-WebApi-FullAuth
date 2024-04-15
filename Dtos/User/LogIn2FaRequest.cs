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
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [MaxLength(36)]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string LoginToken { get; set; } = string.Empty;
    }
}