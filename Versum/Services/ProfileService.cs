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

        public async Task<UserProfileResponseDto?> GetProfileByUsernameAsync(string username, int? claimedUserID)
        {
            return await _db.Users
                .Where(u => u.Username == username)
                .Select(u => new UserProfileResponseDto
                {
                    Username = u.Username,
                    Name = u.Profile.Name ?? "none",
                    Bio = u.Profile.Bio ?? "none",
                    CreatedAt = u.CreatedAt,
                    IsOwner = u.Id == claimedUserID
                })
                .FirstOrDefaultAsync();
        }


        public async Task<(bool success, string? error)> DeleteAndAnonymizeAccount(int userId, DeleteAccountDto deleteDto)
        {
            var user = await _db.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return (false, "Користувача не знайдено.");

            
            bool passwordValid = BCrypt.Net.BCrypt.Verify(deleteDto.Password, user.PasswordHash);
            if (!passwordValid)
                return (false, "Неправильний пароль, введіть ще раз або вийдіть.");

           
            if (deleteDto.ConfirmWord.Trim().ToLower() != "видалити")
                return (false, "Слово підтвердження введено невірно. Введіть «видалити».");

           
            user.Email = $"deleted_{Guid.NewGuid()}@anonymized.com";
            user.Username = $"anon_{Guid.NewGuid():N}"; 
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()); 
            user.IsDeleted = true;

            if (user.Profile != null)
            {
                user.Profile.Name = "Анонімний автор";
                user.Profile.Bio = "Акаунт видалено";
            }

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "Сталася помилка при зверненні до бази даних.");
            }
        }
    }
}
