using System.ComponentModel.DataAnnotations;
namespace Versum
{
    public class UserProfile
    {
        public int Id { get; set; }

        [MaxLength(30)] public string? Name { get; set; }

        [MaxLength(200)] public string? Bio { get; set; }



        public int UserId { get; set; }
        public User User { get; set; } = null!;

    }
}
