using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Versum.Dtos;
using Versum.Models;
using Versum.Context;

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
                return (false, "Чому нас вважають за одну людину?");
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
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.AuthorProfile)
            .FirstOrDefaultAsync(u => u.Username == username);

            bool isCurrentGuest = claimedUserID == null;

            if (user == null) return null;

            return await _db.Users
                .Where(u => u.Username == username)
                .Select(u => new UserProfileResponseDto
                {
                    Username = u.Username,
                    Name = u.Profile.Name ?? "none",
                    Bio = u.Profile.Bio ?? "none",
                    CreatedAt = u.CreatedAt,
                    IsAuthor = (u.AuthorProfile != null),
                    IsOwner = u.Id == claimedUserID,
                    WorksCount = u.AuthorProfile.Posts.Count(p => !p.IsDeleted && !p.IsDraft),
                    FollowingCount = _db.Follows.Count(f => f.FollowerId == u.Id),
                    FollowersCount = _db.Follows.Count(f => f.FollowingId == u.Id),
                    IsFollowing = !isCurrentGuest && _db.Follows.Any(f => f.FollowerId == claimedUserID && f.FollowingId == u.Id)
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

            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);

            user.Email = $"del_{shortGuid}@anon.com"; // Близько 21 символу
            user.Username = $"anon_{shortGuid}";      // 13 символів
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
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"DB_ERROR при видаленні акаунта: {realError}");

                return (false, "Сталася помилка при зверненні до бази даних. Перевірте консоль сервера.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"СЕРВЕРНА_ПОМИЛКА: {ex.Message}");
                return (false, "Сталася непередбачувана помилка на сервері.");
            }



        }

        public async Task<int?> GetUserIdByUsernameAsync(string username)
        {
            return await _db.Users
                .Where(u => u.Username.ToLower() == username.ToLower())
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<(bool success, string? error)> ToggleFollowAsync(int followerId, int followingId)
        {
            if (followerId == followingId) return (false, "Ви не можете підписатися на себе.");

            var existingFollow = await _db.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
            {
                _db.Follows.Remove(existingFollow);
            }
            else
            {
                _db.Follows.Add(new Follow { FollowerId = followerId, FollowingId = followingId });
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }
        public async Task<List<UserFollowDto>> GetFollowingsListAsync(string username)
        {
            return await _db.Follows
                .Where(f => f.Follower.Username == username)
                .Select(f => new UserFollowDto
                {
                    Username = f.Following.Username,
                    DisplayName = f.Following.Profile.Name
                })
                .ToListAsync();
        }

    }

}

