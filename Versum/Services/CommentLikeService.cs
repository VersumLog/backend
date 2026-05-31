using Microsoft.EntityFrameworkCore;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;

namespace Versum.Services
{
    public class CommentLikeService : ICommentLikeService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IProfileService _profileService;

        public CommentLikeService(ApplicationDbContext db, INotificationService notificationService, IProfileService profileService)
        {
            _db = db;
            _notificationService = notificationService;
            _profileService = profileService;
        }

        public async Task<(bool success, string? error)> ToggleLikeAsync(int userId, int postId)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && !p.IsDraft);
            if (post == null) return (false, "PostNotFound");

            var existing = await _db.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
             
            if (existing != null)
            {
                _db.Likes.Remove(existing);
                if (post.LikesCount > 0) post.LikesCount--;
            }
            else
            {
                _db.Likes.Add(new Like { UserId = userId, PostId = postId });
                post.LikesCount++;

                //Notification
                var username = await _profileService.GetUsernameByUserIdAsync(userId);
                await _notificationService.SendLikeNotificationAsync(post.AuthorId, username ?? "Хтось", post.Title);
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<CommentGetDto>> GetCommentsAsync(int postId, int? userId)
        {
            return await _db.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentGetDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Username = c.User.Username,
                    CreatedAt = c.CreatedAt,
                    IsOwner = c.UserId == userId
                })
                .ToListAsync();
        }

        public async Task<(bool success, string? error)> AddCommentAsync(int userId, int postId, CommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return (false, "ContentRequired");

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && !p.IsDraft);
            if (post == null) return (false, "PostNotFound");

            _db.Comments.Add(new Comment
            {
                UserId = userId,
                PostId = postId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            });

            post.CommentsCount++;

            //Notification
            var username = await _profileService.GetUsernameByUserIdAsync(userId);
            await _notificationService.SendCommentNotificationAsync(post.AuthorId, username ?? "Хтось", post.Title);

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> DeleteCommentAsync(int userId, int commentId)
        {
            var comment = await _db.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null) return (false, "CommentNotFound");
            if (comment.UserId != userId) return (false, "NotOwner");

            comment.IsDeleted = true;
            if (comment.Post.CommentsCount > 0) comment.Post.CommentsCount--;

            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}