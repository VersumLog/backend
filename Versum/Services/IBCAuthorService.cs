using Versum.Dtos;

namespace Versum.Services
{

    public interface IBCAuthorService
    {
        Task<(bool Success, string? Error)> BecomeAuthorAsync(int userId, BecomeAuthorDto dto);
        Task<(bool Success, string? Bio, string? Error)> GetAuthorBioAsync(string username);
        Task<(bool Success, string? Error)> UpdateAuthorBioAsync(int userId, BecomeAuthorDto dto);
    }

}
