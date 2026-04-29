using System.ComponentModel.DataAnnotations;
using Versum.Models;
namespace Versum
{
    public class User
    {

        public int Id { get; set; }

        [MaxLength(50)] public string Username { get; set; } = "";

        [MaxLength(60)] public string PasswordHash { get; set; } = "";


        [MaxLength(50)] public string Email { get; set; } = "";


        public bool IsEmailConfirmed { get; set; } = false; // Did user confirm email

        public string? EmailConfirmationTokenHash { get; set; } //Unique token which sends on post for confirmation
        public DateTime? EmailTokenExpiryDate { get; set; } // Limit in Time to conf email
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date and Time when user signed up
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public UserProfile Profile { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Author? AuthorProfile { get; set; }


    }
}
