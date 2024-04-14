using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class SignUpRequest
    {
        [Required]
        [RegularExpression(@"^[a-z0-9](?:[a-z0-9]+[.\-_]?)+[a-z0-9]$", ErrorMessage = "Invalid username format!")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 40 characters long.")]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters!")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(64, MinimumLength = 10, ErrorMessage = "Password must be between 10 and 64 characters long!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\^#@$!%*?&])[A-Za-z\d\^#@$!%*?&]{10,}$", ErrorMessage = "Password must contain at least one number, one special character (^#@$!%*?&), one uppercase and one lower case letter!")]
        public string Password { get; set; } = string.Empty;
    }
}