using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class TokensResponse
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        [Required]
        public string AccessToken { get; set; } = string.Empty;
    }
}