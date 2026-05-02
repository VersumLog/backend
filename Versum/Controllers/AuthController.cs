using global::Versum.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Versum.Context;

namespace Versum.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

   

        [HttpPost("register")]
       
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)// JSON converts to RegisterDtos object
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks all [Required], [RegularExpression] from RegisterDto -> Smth wrong -> returns error 400

            var (success, error, field) = await _authService.RegisterAsync(dto);// calls Service's RegisterAsync method and passes dto

            if (!success)
                return Conflict(new { field, message = error }); //checks if data for transfer does not cause conflicts(error 409)

            return Ok(new { message = "Реєстрація успішна! Перевірте пошту для підтвердження." });
           
        }



        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Токен відсутній");

            var (success, error) = await _authService.ConfirmEmailAsync(token);

            if (!success)
                // redirect on page with error
                return BadRequest(new { message = error });
            /* return Redirect($"https://localhost:7014.com/email-confirmed?success=false&error={Uri.EscapeDataString(error!)}"); */

            // successfull: redirect on main page
            return Ok(new { message = "Email успішно підтверджено!" });
            /* return Redirect("https://localhost:7014.com/email-confirmed?success=true");*/
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)// JSON converts to LoginDto object
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks validation attributes from LoginDto -> Smth wrong -> returns error 400

            var (success, resultMessage, userGmail, username) = await _authService.LoginAsync(dto);// calls Service's LoginAsync method and passes dto

            if (!success)
                return Unauthorized(new { message = resultMessage }); // checks if login fails (wrong password or user) -> returns error 401

            try
            {
                //await _gmailService.SendLoginNotificationAsync(userGmail, username);// awaits email sending to avoid crashes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gmail sending error: {ex.Message}");// logs error but doesn't stop the login process
            }

            return Ok(new { token = resultMessage, message = "Вхід успішний" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotReset([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks validation attributes from LoginDto -> Smth wrong -> returns error 400
            var (success, error) = await _authService.ForgotPasswordAsync(dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }


            return Ok(new { message = "Лист надіслано" });

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks validation attributes from LoginDto -> Smth wrong -> returns error 400
            var (success, error) = await _authService.ResetPasswordAsync(dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }
            return Ok(new { message = "Пароль успішно змінено" });
        }

        [HttpPost("reset-password-token-check")]
        public async Task<IActionResult> ResetPasswordTokenCheck([FromBody] ResetPasswordTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);// checks validation attributes from LoginDto -> Smth wrong -> returns error 400
            
            var (success, error) = await _authService.ResetPasswordTokenCheckAsync(dto);

            if (!success)
            {
                return BadRequest(new { message = error });
            }
            return Ok(new { message = "Токен Підтверджено" });
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
