using System.ComponentModel.DataAnnotations;

namespace Versum.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;
        [MaxLength(2000)] public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
