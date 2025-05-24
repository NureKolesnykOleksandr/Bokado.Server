using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokado.Server.Services
{
    public class EmailConfiguration
    {
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class EmailService
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailService()
        {
            _emailConfig = new EmailConfiguration()
            {
                From = "devlibnure@gmail.com",
                SmtpServer = "smtp.gmail.com",
                Port = 465,
                UserName = "devlibnure@gmail.com",
                Password = "wryz rtor whli rimv"
            };
        }

        public async Task SendEmail(string to, string subject, string content)
        {
            // Создаем email сообщение
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Bokado", _emailConfig.From));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = content };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.Auto);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    throw ex; 
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
