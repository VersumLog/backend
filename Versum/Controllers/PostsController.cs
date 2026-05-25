using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Versum.Dtos;
using Versum.Services;


namespace Versum.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
      
        private readonly IPostService _postService;
        private readonly IProfileService _profileService;

        public PostsController(IPostService postService, IProfileService profileService)
        {
         
            _postService = postService;
            _profileService = profileService;
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
                if (error == "You can't edit published writings") return Conflict(new { message = "Твір уже опубліковано" });
                if (error == "YouAreNotAnOwnerOfDraft") return NotFound(new { message = "Ви нє автором чернетки" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Чернетку збережено",

            });
        }

        //---CRITICAL: Post content is sent every time, though it is not needed. Reminder: Content can have up to 500k letters...
        [HttpGet("drafts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostGetDto>>> GetDrafts([FromQuery] PostQueryDto query)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }
            var drafts = await _postService.GetUserDraftsAsync(authorId, query);
            return Ok(drafts);
        }

        //---CRITICAL: Post content is sent every time, though it is not needed. Reminder: Content can have up to 500k letters...
        [HttpGet("user/{username}")]
        public async Task<ActionResult<IEnumerable<PostGetDto>>> GetPosts([FromRoute] string username, [FromQuery] PostQueryDto query)
        {
            var userId = await _profileService.GetUserIdByUsernameAsync(username);
            if (userId == null)
                return NotFound(new { message = "Користувача не знайдено" });

            var posts = await _postService.GetUserPostsAsync(userId.Value, query);
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

        [HttpPost("add-genre")]
        [Authorize] // Залежно від логіки платформи, пізніше тут можна додати [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGenre([FromBody] GenreCreateDto dto)
        {
            var (success, error) = await _postService.AddGenreAsync(dto.Name);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new { message = "Жанр успішно додано" });
        }
    }

 }
