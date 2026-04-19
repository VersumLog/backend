using System.Net;
using System.Net.Mail;

namespace Versum.Services
{

    public interface IGmailService
    {
        Task SendLoginNotificationAsync(string toGmail, string username);
    }

    public class GmailService : IGmailService
    {
        // Note: for Gmail, use an "App Password" here, not a regular Google password
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderGmail = "your_email@gmail.com";
        private const string SenderPassword = "your_google_app_password";

        public async Task SendLoginNotificationAsync(string toGmail, string username)
        {
            using var client = new SmtpClient(SmtpServer, SmtpPort)
            {
                Credentials = new NetworkCredential(SenderGmail, SenderPassword),
                EnableSsl = true // required for Gmail
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(SenderGmail, "Versum App"),
                Subject = "Успішний вхід у систему",
                Body = $"Привіт, {username}! \n\nВ твій акаунт щойно був здійснений успішний вхід. Якщо це був не ти, негайно зміни пароль.",
                IsBodyHtml = false
            };

            mailMessage.To.Add(toGmail);
            await client.SendMailAsync(mailMessage); // sends email
        }
    }
}