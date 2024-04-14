using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullAuth.Dtos.Email;
using FullAuth.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FullAuth.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettingsDto _emailSettings;
        public EmailService(IOptions<EmailSettingsDto> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(EmailDataDto emailDataDto)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("Recipient Name", emailDataDto.EmailTo));
            message.Subject = emailDataDto.Subject;

            var builder = new BodyBuilder();
            if (emailDataDto.TemplateName != null)
            {
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates/Emails", emailDataDto.TemplateName);
                using (StreamReader reader = File.OpenText(templatePath))
                {
                    builder.HtmlBody = await reader.ReadToEndAsync();
                }

                if (emailDataDto.TemplateData != null)
                {
                    foreach (var item in emailDataDto.TemplateData)
                    {
                        builder.HtmlBody = builder.HtmlBody.Replace(item.Key, item.Value);
                    }
                }
            }
            else
            {
                builder.TextBody = "Email template error!";
            }

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_emailSettings.SmtpServer, _emailSettings.Port, false);
            smtp.Authenticate(_emailSettings.SenderEmail, _emailSettings.Password);
            smtp.Send(message);
            smtp.Disconnect(true);
        }
    }
}