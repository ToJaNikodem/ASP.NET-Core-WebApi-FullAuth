using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class EmailVerificationDto
    {
        [Required]
        public string? EncodedUserId { get; set; }
        [Required]
        public string? VerificationToken { get; set; }
    }
}