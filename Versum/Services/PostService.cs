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
            if (user == null) return new (null, "UserNotFound");
            return (await _db.Posts
        .AsNoTracking()
        .Where(p => p.AuthorId == user.Id)
        .OnlyPublished()
        .ApplySorting(dto.Filter, dto.Ascending)
        .ProjectToPostDto()
        .ToListAsync(), null);
        }

        public async Task<(UserPostsGetDto?, string? Error)> GetPostAsync(int postId)
        {
            var post = await _db.Posts
        .AsNoTracking()
        .Where(p => p.Id == postId)
        .ProjectToPostDto()
        .FirstOrDefaultAsync();
            if (post == null)
            {
                return (null, "Твір не знайдено або він ще не опублікований");
            }

            return (post, null);
        }

        public async Task<(bool Success, string? Error)> UpdateDraftAsync(int postId,int userId, PostDto dto)
        {
            try
            {
                var draft = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);

                if (draft == null) return (false, "DraftNotFound");
                if (draft.AuthorId != userId) return (false, "YouAreNotAnOwnerOfDraft");

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

        public async Task<(bool Success, string? Error)> DeletePostAsync(int userId, int postId)
        {

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.AuthorId == userId && !p.IsDeleted);

            if (post == null) return (false, "PostNotFound");

            post.IsDeleted = true;

            try
            {

                await _db.SaveChangesAsync();
                return (true, null);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeletePostAsync error: {ex.Message}");
                return (false, "ServerError");
            }

        }
    }
} 
