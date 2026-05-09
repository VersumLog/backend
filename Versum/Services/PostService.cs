using Microsoft.EntityFrameworkCore;
using Versum.Dtos;
using Versum.Models;
using Versum.Context;

namespace Versum.Services
{
    public class PostService : IPostService
    {

        private readonly ApplicationDbContext _db;

        public PostService(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<(bool Success, string? Error)> PublishDraftAsync(int postId, int userId)
        {
            try
            {
              
                var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);

                if (post == null) return (false, "PostNotFound");

                if (post.AuthorId != userId) return (false, "YouAreNotAnOwnerOfDraft");

                post.IsDraft = false;
        
                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PublishPostAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }

        public async Task<(bool Success, string? Error, int? PostId)> CreateDraftAsync(int authorId, CreateDraftDto dto)
        {
            try
            {
                var author = await _db.Authors.FirstOrDefaultAsync(a => a.AuthorId == authorId);
                if (author == null) return (false, "AuthorNotFound", null);

                var draftPost = new Post
                {
                    Title = dto.Title,
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
                return (false, "ServerError", null);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateDraftAsync(int postId,int userId, PostDto dto)
        {
            try
            {
                var draft = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.AuthorId == userId);

                if (draft == null) return (false, "DraftNotFound");

                draft.Title = dto.Title;
                draft.Description = dto.Description;
                draft.Content = dto.Content;

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
