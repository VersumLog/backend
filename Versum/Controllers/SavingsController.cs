using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Versum.Dtos;
using Versum.Services;


namespace Versum.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsController : ControllerBase
    {

        private readonly ISavingsService _savingsService;


        public SavingsController(ISavingsService savingsService)
        {

            _savingsService = savingsService;

        }


        [HttpPost("{postId}/save-post")]
        [Authorize]
        public async Task<IActionResult> SavePost(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _savingsService.SavePostAsync(postId, userId);
            if (!success)
            {
                if (error == "PostNotFound") return NotFound(new { message = "Твір не знайдено" });
                if (error == "PostIsSaved") return Conflict(new { message = "Твір уже збережено"});
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Твір успішно збережено"
            });
        }

        [HttpPost("{postId}/unsave-post")]
        [Authorize]
        public async Task<IActionResult> UnsavePost(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _savingsService.UnSavePostAsync(postId, userId);
            if (!success)
            {
                if (error == "PostNotFound") return NotFound(new { message = "Твір не є серед збережених" });
                return BadRequest(new { message = error });
            }

            return Ok( new
            { message = "Твір успішно видалено зі збережених" }
            );
        }

        [HttpGet("get-posts")]
        [Authorize]
        public async Task<IActionResult> GetSavedPosts([FromQuery] PostQueryDto query)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success,savings, error) = await _savingsService.GetSavedPostAsync(userId, query);
            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return Ok(savings);
        }
    }
}
