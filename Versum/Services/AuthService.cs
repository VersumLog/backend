using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Versum.Dtos;

namespace Versum.Services
{
    public class AuthService : IAuthService {

        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        public AuthService(ApplicationDbContext db, IEmailService emailService)
        
        {
            _db = db;
          _emailService = emailService;
        }
        public async Task<(bool Success, string? Error, string? Field)> RegisterAsync(RegisterDto dto)
        {
            bool usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);

            if (usernameExists)
                return (false, "Цей нікнейм вже існує", "username");
        

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            // Hashing password before saving by algorythm
            // BCrypt from NuGet packet BCrypt.Net-Next;

            string token = Guid.NewGuid().ToString("N");
            // Generates unique token for email confirmation
            var confLimit = DateTime.UtcNow.AddHours(24); // email confirmation could be valid only during 24 h

            string tokenHash;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                tokenHash = Convert.ToBase64String(bytes);
            }

            var user = new User
            {
                Username = dto.Username,

                PasswordHash = passwordHash,

                 Email = dto.Email,

                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = confLimit

            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
          

            return (true, null, null);
          

        }
        public async Task<(bool success, string tokenOrError, string userGmail, string username)> LoginAsync(LoginDto dto)
        {
           
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.UsernameOrGmail || u.Username == dto.UsernameOrGmail);

            if (user == null)
            {
                
                return (false, "Невірний логін або пароль", string.Empty, string.Empty);
            }

            
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                
                return (false, "Невірний логін або пароль", string.Empty, string.Empty);
            }

            
            string jwtToken = "dummy_jwt_token_here";

            
            return (true, jwtToken, user.Email, user.Username);
        }

        public async Task<(bool success, string? error)> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return (true, null); //Returning true even if email doesn't exit for account safety
            }
            string ResetToken = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.PasswordResetToken = ResetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1); // Token is valid for one hour after creation

            try
            {
                await _db.SaveChangesAsync();

                //Email message
                await _emailService.SendResetCodeEmailAsync(user.Email, user.Username, ResetToken);
                //await _emailService.SendEmailAsync(user.Email, "Код відновлення пароля", htmlMessage);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Помилка на сервері при обробці запиту");
            }
        }

        public async Task<(bool success, string? error)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);
            if (user == null)
            {
                return (false, "Недійсний токен.");
            }
            if (user.ResetTokenExpires < DateTime.UtcNow)
            {
                return (false, "Термін дії токена вичерпано. Запитуйте відновлення знову.");
            }
            try
            {
                user.PasswordResetToken = null;
                user.ResetTokenExpires = null;
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch(DbUpdateException ex)
            {
                return (false, "Сталася помилка при зверненні до бази даних.");
            }
            catch(Exception ex)
            {
                return (false, "Сталася непередбачувана помилка на сервері.");
            }
        }



    }
}
