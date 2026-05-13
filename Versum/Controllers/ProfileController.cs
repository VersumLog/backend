using global::Versum.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Services;

namespace Versum.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [Authorize]
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks validation attributes from LoginDto -> Smth wrong -> returns error 400

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            var (success, error) = await _profileService.UpdateProfileAsync(currentUserId, dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }
            return Ok(new { message = "Профіль успішно оновлено" });
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int? senderUserId = null;
            if (userIdClaim != null) senderUserId = int.Parse(userIdClaim.Value);

            var profile = await _profileService.GetProfileByUsernameAsync(username.ToLower(), senderUserId);

            if (profile == null)
            {
                return NotFound(new { message = "Користувача не знайдено" });
            }

            return Ok(profile);
        }

        [Authorize]
        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);

            var (success, error) = await _profileService.DeleteAndAnonymizeAccount(currentUserId, dto);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Акаунт успішно видалено та анонімізовано" });
        }

        [Authorize]
        [HttpPost("follow/{username}")]
        public async Task<IActionResult> ToggleFollow(string username)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var targetUserId = await _profileService.GetUserIdByUsernameAsync(username);

            if (targetUserId == null)
            {
                return NotFound(new { message = "Користувача не знайдено" });
            }

            var (success, error) = await _profileService.ToggleFollowAsync(currentUserId, targetUserId.Value);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Статус підписки змінено" });
        }

    }
}
