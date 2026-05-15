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
      
     /*  private readonly IHubContext<NotificationHub> _hubContext; */
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
         
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
        public async Task<IActionResult> UpdateDraft(int postId, [FromBody] PostDto dto)
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
                if (error == "You can't edit published writings") return NotFound(new { message = "Твір уже опубліковано" });
                if (error == "YouAreNotAnOwnerOfDraft") return NotFound(new { message = "Ви нє автором чернетки" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Чернетку збережено",

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
                if (error == "UserNotFound") return NotFound(new { message = "Користувача не знайдено" });
                return BadRequest(new { message = error });
            }

            return Ok(posts);

        }

        [HttpGet("{postId}")] 
        public async Task<IActionResult> GetPostById(int postId)
        {
            //not required to be authorized, but allows you to see your drafts
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);

            var (post, error) = await _postService.GetPostAsync(postId, userId);

            if (error != null)
            {
                return NotFound(new { message = error });
            }

            return Ok(post);
        }


        [HttpPost("{postId}/delete-post")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _postService.DeletePostAsync(userId,postId);
            if (!success)
            {
                if (error == "PostNotFound")
                    return NotFound(new { message = "Твір не знайдено" });
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Твір успішно видалено" });
        }

        [HttpGet("get-genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _postService.GetGenresAsync();
            return Ok(genres);
        }
    }

 }
