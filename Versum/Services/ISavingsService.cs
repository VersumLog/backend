using Versum.Dtos;

namespace Versum.Services
{
    public interface ISavingsService
    {
        Task<(bool Success, string? Error)> SavePostAsync(int postId, int userId);
        Task<(bool Success, string? Error)> UnSavePostAsync(int postId,int userId);
        Task<(bool Success, List<PostGetDto>?, string? Error)>  GetSavedPostAsync(int userId, PostQueryDto query);
    }
}
