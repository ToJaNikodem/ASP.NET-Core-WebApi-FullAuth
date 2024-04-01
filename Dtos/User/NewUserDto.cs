using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class NewUserDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}