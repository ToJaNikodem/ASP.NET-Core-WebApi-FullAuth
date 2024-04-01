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
        public string? OldUserName { get; set; }

        [Required]
        public string? NewUserName { get; set; }
    }
}