using Versum.Dtos;

namespace Versum.Services
{
    public interface IPostService
    {
        Task<(bool Success, string? Error)> PublishDraftAsync(int postId, int userId);
        Task<(bool Success, string? Error, int? PostId)> CreateDraftAsync(int authorId, CreateDraftDto dto);
        Task<(bool Success, string? Error)> UpdateDraftAsync(int postId, int userId, PostDto dto);
    }
}