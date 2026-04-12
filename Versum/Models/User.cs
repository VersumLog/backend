using System.ComponentModel.DataAnnotations;
namespace Versum
{
    public class User
    {

        public int Id { get; set; }

        [MaxLength(50)] public string Username { get; set; } = "";

        [MaxLength(60)] public string PasswordHash { get; set; } = "";


        [MaxLength(50)] public string Gmail { get; set; } = "";


        public bool IsEmailConfirmed { get; set; } = false; // Did user confirm email

        public string? EmailConfirmationToken { get; set; } //Unique token which sends on post for confirmation

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date and Time when user signed up


    }
}
