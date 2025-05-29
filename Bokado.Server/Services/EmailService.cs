using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Net;
using System.Net.Mail;
using System.Reflection;


namespace Bokado.Server.Services
{
    public class EmailService
    {
        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, string to)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Bokado Support", "oleksandr.kolesnyk@nure.ua"));
                message.To.Add(new MailboxAddress(to, recipientEmail));
                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    await client.AuthenticateAsync("oleksandr.kolesnyk@nure.ua", "zvpg beuo lmdj bxxm");

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending error: {ex.Message}");
                return false;
            }
        }
    }
}
