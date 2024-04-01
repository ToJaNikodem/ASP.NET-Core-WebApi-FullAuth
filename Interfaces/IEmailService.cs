using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullAuth.Dtos.Email;

namespace FullAuth.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDataDto emailDataDto);
    }
}