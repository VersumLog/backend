using global::Versum.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Versum.Services;

namespace Versum.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

   
        [HttpPost("register")]
       
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)// JSON converts to RegisterDtos object
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks all [Required], [RegularExpression] from RegisterDto -> Smth wrong -> returns error 400

            var (success, error, field) = await _authService.RegisterAsync(dto);// calls Service's RegisterAsync method and passes dto

            if (!success)
                return Conflict(new { field, message = error }); //checks if data for transfer does not cause conflicts(error 409)

            return Ok(new { message = "Реєстрація успішна" });
           
        }

        // Allow to see added users in the table(only for dev to try it out): shall be deleted or changed.
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
    [FromServices] ApplicationDbContext db)
        {
            return await db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }



    }
}
