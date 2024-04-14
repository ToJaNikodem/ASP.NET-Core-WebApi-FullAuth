using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FullAuth.Dtos.User
{
    public class TwoFactorAuthTokenResponse
    {
        [Required]
        public string LoginToken { get; set; } = string.Empty;
    }
}