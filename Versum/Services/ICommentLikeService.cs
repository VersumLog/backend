using Versum.Dtos;
namespace Versum.Services
{
    public interface ICommentLikeService
    {
        Task<(bool success, string? error)> ToggleLikeAsync(int userId, int postId);
        Task<List<CommentGetDto>> GetCommentsAsync(int postId, int? userId);
        Task<(bool success, string? error)> AddCommentAsync(int userId, int postId, CommentDto dto);
        Task<(bool success, string? error)> DeleteCommentAsync(int userId, int commentId);
    }
}
