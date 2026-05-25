using Microsoft.EntityFrameworkCore;
using Versum.Dtos;
using Versum.Extensions;
using Versum.Context;
using Ganss.Xss;

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

                if (!post.IsDraft) return (false, "AlreadyPublished");

             
                if (string.IsNullOrWhiteSpace(post.Title)) return (false, "TitleRequired");

                if (string.IsNullOrWhiteSpace(post.Description)) return (false, "DescriptionRequired");

                if (string.IsNullOrWhiteSpace(post.Content)) return (false, "ContentRequired");


                post.IsDraft = false;

                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PublishDraftAsync error: {ex.Message}");
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
                Console.WriteLine($"CreateDraftAsync error: {ex.Message}");
                return (false, "ServerError", null);
            }
        }


        //consider combining onto one GetUserPosts
        //---CRITICAL: Post content is sent every time, though it is not needed. Reminder: Content can have up to 500k letters...
        public async Task<List<PostGetDto>?> GetUserDraftsAsync(int authorId, PostQueryDto query)
        {
            return await _db.Posts
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId)
                .OnlyDrafts()
                .Include(p => p.Author)
                    .ThenInclude(a => a.User)
                        .ThenInclude(u => u.Profile)
                .Include(p => p.Genres)
                .ApplySorting(query.Filter, query.Ascending)
                .ProjectToPostDto()
                .ToListAsync();
        }

        public async Task<List<PostGetDto>?> GetUserPostsAsync(int authorId, PostQueryDto query)
        {
            return await _db.Posts
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId)
                .OnlyPublished()
                .Include(p => p.Author)
                    .ThenInclude(a => a.User)
                        .ThenInclude(u => u.Profile)
                .Include(p => p.Genres)
                .ApplySorting(query.Filter, query.Ascending)
                .ProjectToPostDto()
                .ToListAsync();
        }

        public async Task<(PostGetDto?, string? Error)> GetPostAsync(int postId, int? userID)
        {
            var post = await _db.Posts
                .AsNoTracking()
                .Include(p => p.Author)
                    .ThenInclude(a => a.User)
                        .ThenInclude(u => u.Profile)
                .Include(p => p.Genres)
                .Where(p => p.Id == postId)
                .FirstOrDefaultAsync();

            if (post == null || post.IsDeleted || (post.IsDraft && post.AuthorId != userID))
            {
                return (null, "Твір не знайдено або він ще не опублікований");
            }

            return (post.PostToPostGetDto(), null);
        }

        public async Task<(bool Success, string? Error)> UpdateDraftAsync(int postId,int userId, PostDto dto)
        {
            try
            {
                var draft = await _db.Posts
                .Include(p => p.Genres)
                .FirstOrDefaultAsync(p => p.Id == postId);

                if (draft == null) return (false, "DraftNotFound");
                if (draft.IsDraft == false) return (false, "You can't edit published writings");
                if (draft.AuthorId != userId) return (false, "YouAreNotAnOwnerOfDraft");

                var sanitizer = new HtmlSanitizer();
                sanitizer.AllowedAttributes.Add("data-description");
                sanitizer.AllowedAttributes.Add("class");
                sanitizer.AllowedAttributes.Add("id");

                draft.Title = dto.Title;
                draft.Description = dto.Description;
                draft.Content = sanitizer.Sanitize(dto.Content);
                draft.Genres = _db.Genres.Where(g => dto.Genres.Contains(g.Name)).ToList();

                await _db.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateDraftAsync error: {ex.Message}");
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
        public async Task<List<string>> GetGenresAsync()
        {
            return await _db.Genres.Select(g => g.Name).ToListAsync();
        }
    }
}
