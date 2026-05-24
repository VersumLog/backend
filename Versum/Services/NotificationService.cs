using Microsoft.AspNetCore.SignalR;
using Versum.Hubs;

namespace Versum.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendFollowNotificationAsync(int targetUserId, string actorUsername)
        {
            var notification = new NotificationMessage
            {
                Type = "Follower",
                Message = $"{actorUsername} почав(ла) читати вас.",
                ActorUsername = actorUsername
            };

            await _hubContext.Clients.User(targetUserId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
