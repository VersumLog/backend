using Microsoft.EntityFrameworkCore;
using Versum.Dtos;
using Versum.Models;

namespace Versum.Services
{
    public class BCAuthorService

    {
        
        public interface IAuthorService
        {
            Task<(bool Success, string? Error)> BecomeAuthorAsync(int userId, BecomeAuthorDto dto);
            Task<(bool Success, string? Bio, string? Error)> GetAuthorBioAsync(int userId);
        }
        public class AuthorService : IAuthorService
        {
            private readonly ApplicationDbContext _db;

            public AuthorService(ApplicationDbContext db)
            {
                _db = db;
            }



            public async Task<(bool Success, string? Error)> BecomeAuthorAsync(int userId, BecomeAuthorDto dto)
            {
                var user = await _db.Users.Include(u => u.AuthorProfile).FirstOrDefaultAsync(u => u.Id == userId);

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
            public async Task<(bool Success, string? Bio, string? Error)> GetAuthorBioAsync(int userId)
            {
                var author = await _db.Authors
                    .FirstOrDefaultAsync(a => a.AuthorId == userId);

                if (author == null) return (false, null, "NotFound");

                return (true, author.AuthorBio, null);
            }


        }

       
    }
}
