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

        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public BecomeAuthorController(ApplicationDbContext db, IEmailService emailService, IConfiguration configuration)
        {
            _db = db;
            _emailService = emailService;
            _configuration = configuration;
        }


        [HttpPost("become-author-button")]
        [Authorize]
        public async Task<IActionResult> BecomeAuthor([FromBody] BecomeAuthorDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var Id)) return Unauthorized();

           
            var user = await _db.Users.Include(u => u.AuthorProfile).FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) return NotFound();

          
            if (user.AuthorProfile != null)
                return BadRequest("Ви вже є автором.");

            
            user.AuthorProfile = new Author
            {
                AuthorId = user.Id,
                AuthorBio = dto.AuthorBio.Trim()
            };

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Вітаємо, ви стали автором!",
                opportunities = new[]
                 {
            "Можливість писати",
           
                 },
               
            }); ;
        }

        [HttpGet("{Id}/author-bio")]
        public async Task<IActionResult> GetAuthorBio(int Id)
        {
            
            var authorInfo = await _db.Authors
                .FirstOrDefaultAsync(a => a.AuthorId == Id);

            if (authorInfo == null)
                return NotFound("Цей користувач не є автором або профілю не існує.");

            return Ok(new { bio = authorInfo.AuthorBio });
        }

    }
};