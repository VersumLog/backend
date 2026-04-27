using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Versum.Dtos;

namespace Versum.Services
{
    public class ProfileService : IProfileService
    {

        private readonly ApplicationDbContext _db;
        public ProfileService(ApplicationDbContext db)

        {
            _db = db;
        }
        public async Task<(bool success, string? error)> UpdateProfileAsync(int UserId, UserProfileDto dto)
        {
            var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null)
            {
                return (false,"Чому нас вважають за одну людину?");
            }

            bool usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
            if (usernameExists && user.Username != dto.Username)
                return (false, "Цей нікнейм вже існує");

            try
            {
                if (user.Profile == null)
                {
                    user.Profile = new UserProfile();
                }
                if (user.Username != dto.Username)
                {
                    user.Username = dto.Username;
                }
                if (user.Profile.Name != dto.Name)
                {
                    user.Profile.Name = dto.Name;
                }
                if (user.Profile.Bio != dto.Bio)
                {
                    user.Profile.Bio = dto.Bio;
                }
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                return (false, "Сталася помилка при зверненні до бази даних.");
            }
            catch (Exception ex)
            {
                return (false, "Сталася непередбачувана помилка на сервері.");
            }
        }
    }
}
