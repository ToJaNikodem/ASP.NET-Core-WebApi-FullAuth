using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class SignUpDto
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
        [StringLength(64, ErrorMessage = "Password cannot exceed 64 characters!")]
        public string Password { get; set; } = string.Empty;
    }
}