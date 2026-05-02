using System.ComponentModel.DataAnnotations;

namespace Versum
{
    public class Post
    {
        public int Id { get; set; }
        [MaxLength(100)] public string Title { get; set; } = string.Empty;
        [MaxLength(600)] public string Description { get; set; }
        [MinLength(10)][MaxLength(500000)] public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!; // Post's Author
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
