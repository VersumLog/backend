using Versum.Dtos;

namespace Versum.Services
{

        public interface IDictService
        {
            Task<(bool Success, string? Error)> AddPhraseAsync(int userId, int postId, DictDto dto);
            Task<(bool Success,List<DictResponceDto>?, string? Error)> GetPhraseAsync(int userId);

            Task<(bool Success, string? Error)> DeletePhraseAsync(int userId,DeletePhraseDto dto);
    }
 }

