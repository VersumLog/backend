using Versum.Dtos;

public interface IProfileService
{
    Task<(bool success, string? error)> UpdateProfileAsync(int UserId, UserProfileDto dto);
    Task<UserProfileResponseDto?> GetProfileByUsernameAsync(string username, int? claimedUserID);

    Task<(bool success, string? error)> DeleteAndAnonymizeAccount(int userId, DeleteAccountDto deleteDto);

    Task<(bool success, string? error)> ToggleFollowAsync(int followerId, int followingId);

    Task<List<UserFollowDto>> GetFollowingsListAsync(string username);
    Task<List<UserFollowDto>> GetFollowersListAsync(string username);
    Task<int?> GetUserIdByUsernameAsync(string username);
    Task<string?> GetUsernameByUserIdAsync(int userId);
}