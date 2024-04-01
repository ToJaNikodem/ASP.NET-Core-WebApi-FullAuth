using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class PasswordChangeDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        [StringLength(64, ErrorMessage = "Password cannot exceed 64 characters!")]
        public string NewPassword { get; set; } = string.Empty;
    }
}