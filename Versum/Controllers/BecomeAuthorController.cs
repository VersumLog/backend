using global::Versum.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Versum.Models;
using Versum.Services;


namespace Versum.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class BecomeAuthorController : ControllerBase
    {
        private readonly IBCAuthorService _authorService;

        public BecomeAuthorController(IBCAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpPost("become-author-button")]
        [Authorize]
        public async Task<IActionResult> BecomeAuthor([FromBody] BecomeAuthorDto dto)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (success, error) = await _authorService.BecomeAuthorAsync(id, dto);

            if (!success)
            {
                if (error == "NotFound") return NotFound();
                return BadRequest(new { message = error });
            }

            return Ok(new
            {
                message = "Вітаємо, ви стали автором!",
                opportunities = new[]
                {
            "Можливість писати",

                 },

            });
        }

        [HttpGet("{Username}/author-bio")]
        public async Task<IActionResult> GetAuthorBio(string Username)
        {
            var (success, bio, error) = await _authorService.GetAuthorBioAsync(Username);

            if (!success)
            {
                if (error == "NotFound")
                    return NotFound("Цей користувач не є автором або профілю не існує.");
            }

            return Ok(new { bio });
        }
        [HttpPost("update-author-bio")]
        [Authorize]
        public async Task<IActionResult> UpdateAuthorBio([FromBody] BecomeAuthorDto dto)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (success, error) = await _authorService.UpdateAuthorBioAsync(id, dto);

            if (!success)
            {
                if (error == "NotFound") return NotFound("Ви не є автором.");
                if (error == "ServerError") return StatusCode(500, new { message = "Щось пішло не так. Спробуйте пізніше." });
            }

            return Ok(new { message = "Біо успішно оновлено!" });
        }


    }
};