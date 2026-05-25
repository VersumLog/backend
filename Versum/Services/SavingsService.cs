using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Versum.Services
{
    public class SavingsService : ISavingsService
    {

        private readonly ApplicationDbContext _db;

        public SavingsService(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<(bool Success, string? Error)> SavePostAsync(int postId, int userId)
        {

            var post = await _db.Posts.AnyAsync(p => p.Id == postId && !p.IsDraft && !p.IsDeleted);
            if (!post) return (false, "PostNotFound");

            var alreadySaved = await _db.Savings.AnyAsync(s => s.UserId == userId && s.PostId == postId);
            if (alreadySaved) return (false, "PostIsSaved");

            var saved = new Savings
            {
                UserId = userId,
                PostId = postId,
                SavedAt = DateTime.UtcNow

            };

            _db.Savings.Add(saved);
            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SavePostAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }
        public async Task<(bool Success, string? Error)> UnSavePostAsync(int postId, int userId) {

          var savings = await _db.Savings
        .FirstOrDefaultAsync(s => s.UserId == userId && s.PostId == postId);

            if (savings == null) return (false,"PostNotFound"); 

            _db.Savings.Remove(savings);

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnSavePostAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, List<PostGetDto>?, string? Error)> GetSavedPostAsync(int userId)
        {

            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists) return (false, null, "UserNotFound");

          
            var savedPost = await _db.Savings
              .AsNoTracking()
              .Where(s => s.UserId == userId )
              .OrderByDescending(s => s.SavedAt)
              .Select(s => new PostGetDto
              {
                  Title = s.Post.Title,
                  Description = s.Post.Description,
                  Content = s.Post.Content,
                  CreatedAt = s.Post.CreatedAt,
                  Username = s.User.Username,
                  Name = s.User.Profile.Name,
                  Genres = s.Post.Genres.Select(g => g.Name).ToList(),
                  LikesCount = s.Post.LikesCount,
                  CommentsCount = s.Post.CommentsCount,
                  IsLikedByUser = s.Post.Likes.Any(l => l.UserId == userId)
              })
               .ToListAsync();

            return (true, savedPost, null);

        }

    }
}
