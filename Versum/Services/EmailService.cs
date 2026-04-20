using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var emailMessage = new MimeKit.MimeMessage();

        // 1. Формуємо заголовок листа
        emailMessage.From.Add(new MailboxAddress("Versum", emailSettings["SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;

        // 2. Додаємо вміст (HTML)
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = htmlMessage
        };

        // 3. Відправка через Mailtrap
        using (var client = new SmtpClient())
        {
            // Підключаємося до сервера Mailtrap
            await client.ConnectAsync(
                emailSettings["SmtpServer"],
                int.Parse(emailSettings["Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            // Авторизуємося вашим Username та Password
            await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);

            // Надсилаємо
            await client.SendAsync(emailMessage);

            // Розриваємо з'єднання
            await client.DisconnectAsync(true);
        }
    }
}