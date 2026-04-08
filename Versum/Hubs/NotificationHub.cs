using Microsoft.AspNetCore.SignalR;

namespace Versum.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("NewPostPublished", message);
        }

        // track users
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"User Connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}