using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Versum.Hubs
{
    public class NotificationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string ActorUsername { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }

    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task SendNotificationToUser(string userId, NotificationMessage notification)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"User Connected: {Context.UserIdentifier} with ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}