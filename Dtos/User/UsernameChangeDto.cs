using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class UsernameChangeDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[a-z0-9](?:[a-z0-9]+[.\-_]?)+[a-z0-9]$", ErrorMessage = "Invalid username format!")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 40 characters long.")]
        public string NewUserName { get; set; } = string.Empty;
    }
}