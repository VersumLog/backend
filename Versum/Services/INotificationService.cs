public interface INotificationService
{
    Task SendFollowNotificationAsync(int targetUserId, string actorUsername);
}