using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Core.Enums;
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
        public async Task<ActionResult<IEnumerable<UserPostsGetDto>>> GetDrafts(
            [FromQuery] FilterOptions filter,
            [FromQuery] bool ascending)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }
            var drafts = await _postService.GetUserDraftsAsync(authorId, filter, ascending);
            return Ok(drafts); //Користувачі, які не мають ролі автора або не мають створених чернеток отримують порожній список
        }

        [HttpGet("get-posts")]
        public async Task<ActionResult<IEnumerable<UserPostsGetDto>>> GetPosts(
            [FromQuery] UserPostsRequestDto dto
            )
        {
            var (posts, error) = await _postService.GetUserPostsAsync(dto);
            if (error != null)
            {
                return NotFound(new { message = error });
            }

            return Ok(posts);

        }
    }
}
