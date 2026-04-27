using global::Versum.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var profile = await _profileService.GetProfileByUsernameAsync(username.ToLower());

            if (profile == null)
            {
                return NotFound(new { message = "Користувача не знайдено" });
            }

            return Ok(profile);
        }
    }
}
