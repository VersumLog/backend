using Versum.Dtos;

public interface INotificationService
{
    Task SendFollowNotificationAsync(int targetUserId, string actorUsername);
    Task<List<NotificationDto>?> GetNotificationsAsync(int userId);
    Task<(bool Success, string? Error)> ReadNotificationAsync(int id, int userId);
}