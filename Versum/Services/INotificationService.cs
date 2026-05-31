using Versum.Dtos;

public interface INotificationService
{
    Task SendFollowNotificationAsync(int targetUserId, string actorUsername);
    Task SendLikeNotificationAsync(int targetUserId, string actorUsername, string postName);
    Task SendCommentNotificationAsync(int targetUserId, string actorUsername, string postName);
    Task NotifyFollowersAboutPublishingAsync(int authorId, string actorUsername, string postName);
    Task SendNewPostNotificationAsync(int targetUserId, string actorUsername, string postName);
    Task<List<NotificationDto>?> GetNotificationsAsync(int userId);
    Task<(bool Success, string? Error)> ReadNotificationAsync(int id, int userId);
}