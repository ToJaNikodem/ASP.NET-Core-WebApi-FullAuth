using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class DeleteUserDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}