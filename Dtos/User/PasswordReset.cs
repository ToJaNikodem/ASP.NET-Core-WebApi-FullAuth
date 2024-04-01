using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class PasswordResetDto
    {
        [Required]
        public string EncodedUserId { get; set; } = string.Empty;
        [Required]
        public string ResetToken { get; set; } = string.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "Password cannot exceed 64 characters!")]
        public string NewPassword { get; set; } = string.Empty;
    }
}