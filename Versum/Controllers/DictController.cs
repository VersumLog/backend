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
    public class DictController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDictService _dictService;

        public DictController(ApplicationDbContext context, IDictService dictService)
        {
            _context = context;
            _dictService = dictService;
        }




        [HttpPost("{postId}/add-phrase")]
        [Authorize]
        public async Task<IActionResult> AddPhrase(int postId, [FromBody] DictDto dto)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _dictService.AddPhraseAsync(userId, postId, dto);

            if (!success)
            {
                if (error == "PostNotFound") return NotFound(new { message = "Твір не знайдено" });
                if (error == "UserNotFound") return NotFound(new { message = "Користувача не знайдено" });
                if (error == "PhraseAlreadyExists") return BadRequest(new { message = "Це слово вже є у вашому словнику" });
                return BadRequest(new { message = error });
            }

            return StatusCode(201, new
            {
                message = "Додано до словника",

            });
        }

        [HttpPost("delete-phrase")]
        [Authorize]
        public async Task<IActionResult> DeletePhrase([FromBody] DeletePhraseDto dto)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, error) = await _dictService.DeletePhraseAsync(userId, dto);
            if (!success)
            {
                if (error == "PhraseNotFound")
                    return NotFound(new { message = "Не знайдено" });
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Успішно видалено" });
        }

        [HttpGet("get-dictionary")]
        [Authorize]
        public async Task<IActionResult> GetPhrase()
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success,phrases,error) = await _dictService.GetPhraseAsync(userId);

            if (!success)
            {
                if (error == "UserNotFound") return NotFound(new { message = "Користувача не знайдено" });
        
                return BadRequest(new { message = error });
            }

            return Ok(phrases);
        }




    }
}

