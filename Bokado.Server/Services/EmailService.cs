using MimeKit;

namespace Bokado.Server.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, string recipientName)
        {
            try
            {
                var fromAddress = _configuration["Email:From"];
                var password = _configuration["Email:Password"];
                var displayName = _configuration["Email:DisplayName"] ?? "Bokado Support";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(displayName, fromAddress));
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(fromAddress, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

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
