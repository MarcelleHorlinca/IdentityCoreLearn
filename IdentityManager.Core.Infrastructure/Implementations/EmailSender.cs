using IdentityManager.Core.Infrastructure.Services;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using IdentityManager.Core.Infrastructure.Interfaces;

namespace IdentityManager.Core.Infrastructure.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.UserName, _emailSettings.UserName));
            message.To.Add(new MailboxAddress("Marcelle", email));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(_emailSettings.UserName, _emailSettings.Password);
                client.Send(message);
                client.Disconnect(true);
            }

            return Task.CompletedTask;
        }

    }
}


/*

    using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using IdentityManager.Core.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;


namespace IdentityManager.Core.Infrastructure.Implementations
{



    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
            string smtpUsername = _configuration["EmailSettings:UserName"];
            string smtpPassword = _configuration["EmailSettings:Password"];
            string fromEmail = _configuration["EmailSettings:From"];
            string fromName = "Marcelle";

            var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

*/


