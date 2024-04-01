using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullAuth.Models;

namespace FullAuth.Interfaces
{
    public interface ITokenService
    {
        (string, string) CreateTokens(User user);
    }
}