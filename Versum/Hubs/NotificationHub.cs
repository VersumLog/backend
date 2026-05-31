using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Versum.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"User Connected: {Context.UserIdentifier} with ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
    }
}