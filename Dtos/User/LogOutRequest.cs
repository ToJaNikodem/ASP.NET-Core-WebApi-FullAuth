using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class LogOutRequest
    {
        [Required]
        [MaxLength(36)]
        public string UserId { get; set; } = string.Empty;
    }
}