using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class RefreshTokenDto
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? RefreshToken { get; set; }
    }
}