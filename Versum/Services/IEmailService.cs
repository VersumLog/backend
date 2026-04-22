public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    Task SendResetCodeEmailAsync(string toEmail, string userName, string code);
}