using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Versum.Dtos;

namespace Versum.Services
{
    public class AuthService : IAuthService {

        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public AuthService(ApplicationDbContext db,IEmailService emailService,IConfiguration configuration)
        {
            _db = db;
            _emailService = emailService;
            _configuration = configuration;
        }
        public async Task<(bool Success, string? Error, string? Field)> RegisterAsync(RegisterDto dto)
        {
            bool usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
            bool emailExists = await _db.Users.AnyAsync(e => e.Email == dto.Email);

            if (usernameExists)
                return (false, "Цей нікнейм вже існує", "username");
            if (emailExists)
                return (false, "Цей імейл вже існує", "email");


            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            // Hashing password before saving by algorythm
            // BCrypt from NuGet packet BCrypt.Net-Next;


            string registerToken = Guid.NewGuid().ToString();

            // Generates unique token for email confirmation
            string RegisterTokenHash;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(registerToken));
                RegisterTokenHash = Convert.ToBase64String(bytes);
            }
          
            var confLimit = DateTime.UtcNow.AddHours(24); // email confirmation could be valid only during 24 h
            
         

            var user = new User
            {
                Username = dto.Username,

                PasswordHash = passwordHash,

                 Email = dto.Email,

                EmailConfirmationTokenHash = RegisterTokenHash,
                EmailTokenExpiryDate = confLimit

            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();


            var confirmLink = $"{_configuration["AppSettings:BaseUrl"]}/api/Auth/confirm-email?token={Uri.EscapeDataString(registerToken)}&email={dto.Email}";
            var filePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ConfRegistrationTemplate.html");


            string htmlBody = await File.ReadAllTextAsync(filePath);
            htmlBody = htmlBody.Replace("{Username}", dto.Username)
                               .Replace("{confirmLink}", confirmLink);

            await _emailService.SendEmailAsync(dto.Email, "Підтвердження реєстрації — Versum", htmlBody);


            return (true, null, null);
          

        }

        public async Task<(bool success, string? error)> ConfirmEmailAsync(string token)
        {
            string incomingHash;
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                incomingHash = Convert.ToBase64String(bytes);
            }

           
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.EmailConfirmationTokenHash == incomingHash &&
                u.EmailTokenExpiryDate > DateTime.UtcNow
            );

            if (user is null)
                return (false, "Посилання недійсне або термін дії вичерпано");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationTokenHash = null;
            user.EmailTokenExpiryDate = null;

            await _db.SaveChangesAsync();

            return (true, null);
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


            string jwtToken = GenerateJwtToken(user);


            return (true, jwtToken, user.Email, user.Username);
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // currentUserId
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Помилка на сервері при обробці запиту");
            }
        }

        public async Task<(bool success, string? error)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Email == dto.Email
                && u.PasswordResetToken == dto.Token
                );
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
                user.PasswordHash = passwordHash;

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

        public async Task<(bool success, string? error)> ResetPasswordTokenCheckAsync(ResetPasswordTokenDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Email == dto.Email
                && u.PasswordResetToken == dto.Token
                );
            if (user == null)
            {
                return (false, "Недійсний токен.");
            }
            if (user.ResetTokenExpires < DateTime.UtcNow)
            {
                return (false, "Термін дії токена вичерпано. Запитуйте відновлення знову.");
            }
                return (true, null);

        }

    }
}
