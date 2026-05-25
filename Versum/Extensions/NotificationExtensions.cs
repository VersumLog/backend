using System.Linq;
using Versum.Core.Enums;
using Versum.Dtos;
using Versum.Models;

namespace Versum.Extensions;

public static class NotificationExtensions
{
    public static IQueryable<NotificationDto> NotificationsToDto(this IQueryable<Notification> query)
    {
        return query.Select(p => NotificationToDto(p));
    }
    public static NotificationDto NotificationToDto(this Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Message = n.Message ?? "щось",
            ActorUsername = n.ActorUsername ?? "хтось",
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}