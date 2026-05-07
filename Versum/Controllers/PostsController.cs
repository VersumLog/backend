using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Core.Enums;
using Versum.Dtos;
using Versum.Hubs;
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




        [HttpPost("create-post")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] PostDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }

            var (success, error) = await _postService.PublishPostAsync(authorId, dto);
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
        public async Task<IActionResult> CreateDraft([FromBody] PostDto dto)
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
                message = "Чернетку збережено",
                postId = postId
            });
        }

        [HttpGet("get-drafts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Post>>> GetDrafts(
            [FromQuery] FilterOptions filter,
            [FromQuery] bool ascending)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);



            var drafts = await _postService.GetUserDraftsAsync(currentUserId, filter, ascending);

            if (drafts == null)
            {
                return NotFound(new { message = "Чернетки не знайдено" });
            }

            return Ok(drafts);

        }

        [HttpGet("get-posts")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts(
            [FromQuery] UserPostsRequestDto dto
            )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var drafts = await _postService.GetUserPostsAsync(dto);

            if (drafts == null)
            {
                return NotFound(new { message = "Чернетки не знайдено" });
            }

            return Ok(drafts);

        }
    }
}
