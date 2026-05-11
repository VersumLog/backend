using Versum.Core.Enums;
using Versum.Dtos;

namespace Versum.Services
{
    public interface IPostService
    {
        Task<(bool Success, string? Error)> DeletePostAsync(int userId, int postId);     
        Task<List<UserPostsGetDto>> GetUserDraftsAsync(int claimedUserID, FilterOptions filter, bool ascending);
        Task<(List<UserPostsGetDto>?, string? Error)> GetUserPostsAsync(UserPostsRequestDto dto);
        Task<(bool Success, string? Error)> PublishDraftAsync(int postId, int userId);
        Task<(bool Success, string? Error, int? PostId)> CreateDraftAsync(int authorId, CreateDraftDto dto);
        Task<(bool Success, string? Error)> UpdateDraftAsync(int postId, int userId, PostDto dto);
        Task<List<string>> GetGenresAsync();
    }
}