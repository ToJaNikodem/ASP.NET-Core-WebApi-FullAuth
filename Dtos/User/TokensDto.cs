using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class TokensDto
    {
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
    }
}