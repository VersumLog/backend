using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;
using Versum.Extensions;
using Ganss.Xss;


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

            var saved = new Saving
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

        public async Task<(bool Success, List<PostGetDto>?, string? Error)> GetSavedPostAsync(int userId, PostQueryDto query)
        {
            var savings = await _db.Savings
             .AsNoTracking()
             .Where(s => s.UserId == userId)
             .OrderByDescending(s => s.SavedAt)
             .Select(s => s.Post)
             .ApplySorting(query.Filter, query.Ascending)
             .ProjectToPostDto(userId)
             .ToListAsync();

            return (true, savings, null);
        }
    }
}
