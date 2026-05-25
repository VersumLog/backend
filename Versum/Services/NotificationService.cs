using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Versum.Context;
using Versum.Dtos;
using Versum.Hubs;
using Versum.Models;
using Versum.Extensions;

namespace Versum.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _db;

        public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext db)
        {
            _hubContext = hubContext;
            _db = db;
        }

        public async Task SendFollowNotificationAsync(int targetUserId, string actorUsername)
        {
            var notification = new Notification
            {
                TargetUserId = targetUserId,
                Type = "Follower",
                Message = $"{actorUsername} почав(ла) читати вас.",
                ActorUsername = actorUsername,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _hubContext.Clients.User(targetUserId.ToString())
                .SendAsync("ReceiveNotification", notification.NotificationToDto());
        }

        public async Task<List<NotificationDto>?> GetNotificationsAsync(int userId)
        {
            return await _db.Notifications
                .AsNoTracking()
                .Where(n => n.TargetUserId == userId)
                .OrderByDescending(n => n.Id)
                .NotificationsToDto()
                .ToListAsync();
        }
        public async Task<(bool Success, string? Error)> ReadNotificationAsync(int id, int userId)
        {
            try
            {

                var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id);

                if (notification == null) return (false, "NotificationNotFound");

                if (notification.TargetUserId != userId) return (false, "It is not sent to you :(");

                if (notification.IsRead) return (false, "AlreadyRead");



                notification.IsRead = true;

                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadNotificationAsync error: {ex.Message}");
                return (false, "ServerError");
            }
        }
    }
}