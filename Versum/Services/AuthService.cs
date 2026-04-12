using Versum.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Versum.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, string? Field)> RegisterAsync(RegisterDto dto);
        // Metjod returns:
        // bool Success = successful registration
        // string? Error = text of error (or null if everuthing is ok)
        // string? Field = what field has error (or null if everything is ok)

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
            // Generateі unique token for email confirmation
    

            var user = new User
            {
                Username = dto.Username,
               
                PasswordHash = passwordHash,
               
                Gmail = dto.Gmail,
               
                EmailConfirmationToken = token
                
               
            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
          

            return (true, null, null);
          
        }




    }
}
