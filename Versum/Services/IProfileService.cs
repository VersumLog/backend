using Versum.Dtos;

public interface IProfileService
{
    Task<(bool success, string? error)> UpdateProfileAsync(int UserId, UserProfileDto dto);
    Task<UserProfileResponseDto?> GetProfileByUsernameAsync(string username, int? claimedUserID);
}