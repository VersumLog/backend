using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;

namespace Versum.Services
{
    public class DictService : IDictService
    {
        private readonly ApplicationDbContext _db;
        public DictService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string? Error)> AddPhraseAsync(int userId, DictDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return (false, "UserNotFound");

            var post = await _db.Posts.AnyAsync(p => p.Id == dto.PostId && !p.IsDraft && !p.IsDeleted);
            if (post == false) return (false, "PostNotFound");

            var exists = await _db.Dictionary.AnyAsync(d => d.UserId == userId && d.PostId == dto.PostId && d.AnchorId == dto.AnchorId);
            if (exists) return (false, "PhraseAlreadyExists");

            var phrase = new Dictionary
            {
                UserId = userId,
                PostId = dto.PostId,
                Phrase = dto.Phrase,
                Description = dto.Description,
                AnchorId = dto.AnchorId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Dictionary.Add(phrase);
            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddPhraseAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, string? Error)> DeletePhraseAsync(int userId, DeletePhraseDto dto)
        {
            var phrase = await _db.Dictionary
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Id == dto.Id && !p.IsDeleted);

            if (phrase == null) return (false, "PhraseNotFound");

            phrase.IsDeleted = true;

            try
            {
                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeletePhraseAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, List<DictResponceDto>?, string? Error)> GetDictionaryAsync(int userId)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists) return (false, null, "UserNotFound");

            var phrases = await _db.Dictionary
              .AsNoTracking()
              .Where(d => d.UserId == userId && !d.IsDeleted)
              .OrderByDescending(d => d.CreatedAt)
              .Select(d => new DictResponceDto
              {
                  Id = d.Id,
                  PostId = d.PostId,
                  PostTitle = d.Post != null ? d.Post.Title : string.Empty,
                  Phrase = d.Phrase,
                  Description = d.Description,
                  AnchorId = d.AnchorId,
                  CreatedAt = d.CreatedAt
              })
               .ToListAsync();

            return (true, phrases, null);
        }
    }
}