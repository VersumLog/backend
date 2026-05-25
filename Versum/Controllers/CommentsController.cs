using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Versum.Dtos;
using Versum.Services;

namespace Versum.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentLikeService _commentLikeService;

        public CommentsController(ICommentLikeService commentLikeService)
        {
            _commentLikeService = commentLikeService;
        }

        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, error) = await _commentLikeService.ToggleLikeAsync(userId, postId);
            if (!success)
            {
                if (error == "PostNotFound") return NotFound(new { message = "Твір не знайдено" });
                return BadRequest(new { message = error });
            }
            return Ok(new { message = "Лайк оновлено" });
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);

            var comments = await _commentLikeService.GetCommentsAsync(postId, userId == 0 ? null : userId);
            return Ok(comments);
        }

        [HttpPost("{postId}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, error) = await _commentLikeService.AddCommentAsync(userId, postId, dto);
            if (!success)
            {
                if (error == "PostNotFound") return NotFound(new { message = "Твір не знайдено" });
                return BadRequest(new { message = error });
            }
            return StatusCode(201, new { message = "Коментар додано" });
        }

        [HttpPost("comments/{commentId}/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var (success, error) = await _commentLikeService.DeleteCommentAsync(userId, commentId);
            if (!success)
            {
                if (error == "CommentNotFound") return NotFound(new { message = "Коментар не знайдено" });
                if (error == "NotOwner") return Forbid();
                return BadRequest(new { message = error });
            }
            return Ok(new { message = "Коментар видалено" });
        }
    }
}