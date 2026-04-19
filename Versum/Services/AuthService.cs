using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Versum.Dtos;

namespace Versum.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, string? Field)> RegisterAsync(RegisterDto dto);
        // Method returns:
        // bool Success = successful registration
        // string? Error = text of error (or null if everuthing is ok)
        // string? Field = what field has error (or null if everything is ok)

        Task<(bool success, string tokenOrError, string userGmail, string username)> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService {

        private readonly ApplicationDbContext _db;
        public AuthService(ApplicationDbContext db)
        
        {
            _db = db;
          
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
                u.Gmail == dto.UsernameOrGmail || u.Username == dto.UsernameOrGmail);

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

            
            return (true, jwtToken, user.Gmail, user.Username);
        }





    }
}
