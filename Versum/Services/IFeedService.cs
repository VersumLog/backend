using Versum.Dtos;

namespace Versum.Services
{
    public interface IFeedService
    {
        Task<List<PostGetDto>> GetSmartFeedAsync(int? currentUserId, int limit = 20, int skip = 0);
    }
}