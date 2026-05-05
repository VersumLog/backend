using Versum.Dtos;

namespace Versum.Services
{
    public interface IPostService
    {
        Task<(bool Success, string? Error)> PublishPostAsync(int Id, PostDto dto);
        Task<(bool Success, string? Error, int? PostId)> CreateDraftAsync(int authorId, PostDto dto);
    }
}