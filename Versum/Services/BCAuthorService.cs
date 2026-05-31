using Microsoft.EntityFrameworkCore;
using Versum.Dtos;
using Versum.Models;
using Versum.Context;

namespace Versum.Services
{


    public class BCAuthorService : IBCAuthorService
    {
        private readonly ApplicationDbContext _db;

        public BCAuthorService(ApplicationDbContext db)
        {
            _db = db;
        }



        public async Task<(bool Success, string? Error)> BecomeAuthorAsync(int userId, BecomeAuthorDto dto)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.AuthorProfile)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return (false, "NotFound");
                if (user.AuthorProfile != null) return (false, "Ви вже є автором.");

                user.AuthorProfile = new Author
                {
                    AuthorId = user.Id,
                    AuthorBio = dto.AuthorBio.Trim()
                };

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BecomeAuthorAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, string? Bio, string? Error)> GetAuthorBioAsync(string username)
        {
            try
            {
                var author = await _db.Authors
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.User.Username == username);

                if (author == null) return (false, null, "NotFound");

                return (true, author.AuthorBio, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAuthorBioAsync error: {ex.Message}");
                return (false, null, "ServerError");
            }


        }
        public async Task<(bool Success, string? Error)> UpdateAuthorBioAsync(int userId, BecomeAuthorDto dto)
        {
            try
            {
                var author = await _db.Authors
                    .FirstOrDefaultAsync(a => a.AuthorId == userId);

                if (author == null) return (false, "NotFound");

                author.AuthorBio = dto.AuthorBio.Trim();

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateAuthorBioAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }


    }
}
