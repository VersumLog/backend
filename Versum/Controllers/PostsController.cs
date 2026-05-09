using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Dtos;
using Versum.Hubs;
using Versum.Context;
using Versum.Services;


namespace Versum.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IPostService _postService;

        public PostsController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IPostService postService)
        {
            _context = context;
            _hubContext = hubContext;
            _postService = postService;
        }




        [HttpPost("{postId}/publish-draft")]
        [Authorize]
        public async Task<IActionResult> PublishDraft(int postId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _postService.PublishDraftAsync(postId, userId);
            if (!success)
            {
                if (error == "AuthorNotFound") return NotFound(new { message = "Профіль автора не знайдено" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Твір успішно опубліковано"
            });

        }


        [HttpPost("create-draft")]
        [Authorize]
        public async Task<IActionResult> CreateDraft([FromBody] CreateDraftDto dto)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }

            var (success, error, postId) = await _postService.CreateDraftAsync(authorId, dto);

            if (!success)
            {
                if (error == "AuthorNotFound") return NotFound(new { message = "Профіль автора не знайдено" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Чернетку створено",
                postId = postId
            });
        }

        [HttpPost("{postId}/update-draft")]
        [Authorize]
        public async Task<IActionResult> UpdateDraft(int postId,[FromBody] PostDto dto)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _postService.UpdateDraftAsync(postId, userId, dto);

            if (!success)
            {
                if (error == "DraftNotFound") return NotFound(new { message = "Чернетку не знайдено" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Чернетку збережено",
 
            });
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}
