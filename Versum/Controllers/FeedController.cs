using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Versum.Dtos;
using Versum.Services;

namespace Versum.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService)
        {
            _feedService = feedService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PostGetDto>>> GetFeed([FromQuery] int limit = 20)
        {
            int? currentUserId = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int parsedId))
                {
                    currentUserId = parsedId;
                }
            }

            var feed = await _feedService.GetSmartFeedAsync(currentUserId, limit);

            return Ok(feed);
        }
    }
}