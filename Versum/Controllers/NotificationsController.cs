using global::Versum.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Context;
using Versum.Services;

namespace Versum.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {

        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<ActionResult> ReadNotification([FromRoute] int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _notificationService.ReadNotificationAsync(id, userId);

            if (!success)
            {
                if (error == "NotificationNotFound")
                    return NotFound(new { message = "Сповіщення не знайдено" });
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Сповіщення прочитано..." });
        }
    }
}
