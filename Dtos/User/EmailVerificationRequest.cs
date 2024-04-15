using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class EmailVerificationRequest
    {
        [Required]
        [MaxLength(64)]
        public string EncodedUserId { get; set; } = string.Empty;
        [Required]
        [MaxLength(300)]
        public string VerificationToken { get; set; } = string.Empty;
    }
}