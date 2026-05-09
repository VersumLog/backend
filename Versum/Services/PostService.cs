using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Versum.Dtos;
using Versum.Models;
using Versum.Core.Enums;
using Versum.Extensions;
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

        public async Task<(bool Success, string? Error, int? PostId)> CreateDraftAsync(int authorId, PostDto dto)
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
                return (false, "ServerError", null);
            }
        }

        public async Task<List<UserPostsGetDto>> GetUserDraftsAsync(int authorId, FilterOptions filter, bool ascending)
        {
            return await _db.Posts
        .AsNoTracking()
        .Where(p => p.AuthorId == authorId)
        .OnlyDrafts()
        .ApplySorting(filter, ascending)
        .ProjectToPostDto()
        .ToListAsync();
        }

        public async Task<(List<UserPostsGetDto>?, string? Error)> GetUserPostsAsync(UserPostsRequestDto dto)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return new (null, "Користувача не знайдено");
            return (await _db.Posts
        .AsNoTracking()
        .Where(p => p.AuthorId == user.Id)
        .OnlyPublished()
        .ApplySorting(dto.Filter, dto.Ascending)
        .ProjectToPostDto()
        .ToListAsync(), null);
        }

        }
}