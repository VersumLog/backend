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

        private IQueryable<UserDraftsGetDto> MapToDraftDto(IQueryable<Post> query)
        {
            return query.Select(p => new UserDraftsGetDto
            {
                PostId = p.Id,
                Title = p.Title,
                Description = p.Description ?? "none",
                Content = p.Content ?? "none",
                CreatedAt = p.CreatedAt,
                Username = p.Author.User.Username,
                Name = p.Author.User.Profile.Name ?? "none",
                Genres = p.Genres
            });
        }

        public async Task<List<UserDraftsGetDto>?> GetUserDraftsAsync(int? claimedUserID, FilterOptions filter, bool ascending)
        {
            var author = await _db.Authors
            .FirstOrDefaultAsync(a => a.User.Id == claimedUserID);
            if (author == null) return null;
            var query = _db.Posts
        .Where(p => p.AuthorId == author.AuthorId)
        .OnlyDrafts()
        .ApplySorting(filter, ascending);

            return await MapToDraftDto(query).ToListAsync();
        }

        public async Task<List<UserDraftsGetDto>?> GetUserPostsAsync(UserPostsRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return null;
            var author = await _db.Authors
            .FirstOrDefaultAsync(a => a.User.Id == user.Id);
            if (author == null) return null;
            var query = _db.Posts
        .Where(p => p.AuthorId == author.AuthorId)
        .OnlyPublished()
        .ApplySorting(dto.Filter, dto.Ascending);

            return await MapToDraftDto(query).ToListAsync();
        }

        }
}