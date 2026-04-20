using Microsoft.AspNetCore.Hosting;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public EmailService(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public async Task SendResetCodeEmailAsync(string toEmail, string userName, string code)
    {
        string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "ForgotPasswordTemplate.html");

        string htmlContent = await File.ReadAllTextAsync(templatePath);

        htmlContent = htmlContent
            .Replace("{UserName}", userName)
            .Replace("{RESET_TOKEN}", code);

        await SendEmailAsync(toEmail, "Ваш код для зміни пароля", htmlContent);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var emailMessage = new MimeKit.MimeMessage();

        // Email Message Creation
        emailMessage.From.Add(new MailboxAddress("Versum", emailSettings["SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;

        // 2. Email body
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = htmlMessage
        };

        // Mailtrap
        using (var client = new SmtpClient())
        {
            // Connecting to Mailtrap server
            await client.ConnectAsync(
                emailSettings["SmtpServer"],
                int.Parse(emailSettings["Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            // Client Auth
            await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);

            // Sending an email
            await client.SendAsync(emailMessage);

            // disconnecting
            await client.DisconnectAsync(true);
        }
    }
}