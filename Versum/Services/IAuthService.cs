using Versum.Dtos;

public interface IAuthService
{
    Task<(bool Success, string? tokenOrError, string? Field)> RegisterAsync(RegisterDto dto);
    // Method returns:
    // bool Success = successful registration
    // string? Error = text of error (or null if everuthing is ok)
    // string? Field = what field has error (or null if everything is ok)

    Task<(bool success, string? error)> ConfirmEmailAsync(string token);
    Task<(bool success, string tokenOrError, string userGmail, string username)> LoginAsync(LoginDto dto);

    Task<(bool success, string? error)> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<(bool success, string? error)> ResetPasswordAsync(ResetPasswordDto dto);
    Task<(bool success, string? error)> ResetPasswordTokenCheckAsync(ResetPasswordTokenDto dto);
}