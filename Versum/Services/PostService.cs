using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;

namespace Versum.Services
{
    public class PostService : IPostService
    {

        private readonly ApplicationDbContext _db;

        public PostService(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<(bool Success, string? Error)> PublishPostAsync(int authorId, PostDto dto)
        {
            try
            {
                var author = await _db.Authors.FirstOrDefaultAsync(u => u.AuthorId == authorId);

                if (author == null) return (false, "AuthorNotFound");

                var newPost = new Post
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Content = dto.Content,
                    AuthorId = author.AuthorId,
                   CreatedAt = DateTime.UtcNow
                   
                };

                _db.Posts.Add(newPost);

               
                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PublishPostAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, string? Error, int? PostId)> SaveDraftAsync(int authorId, PostDto dto)
        {
            try
            {
                var author = await _db.Authors.FirstOrDefaultAsync(a => a.AuthorId == authorId);
                if (author == null) return (false, "AuthorNotFound", null);

                var draftPost = new Post
                {
                    Title = dto.Title,
                    Description = dto.Description ?? string.Empty,
                    Content = dto.Content ?? string.Empty,
                    AuthorId = authorId,
                    IsDraft = true, 
                    CreatedAt = DateTime.UtcNow
                };

                _db.Posts.Add(draftPost);
                await _db.SaveChangesAsync();

                return (true, null, draftPost.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveDraftAsync error: {ex.Message}");
                return (false, "ServerError",null);
            }
        }

    }
}
