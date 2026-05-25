namespace Versum.Models
{
    public class PostReaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;
        public bool IsLiked { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public float PriorityScore { get; set; } = 0f;
        public DateTime LastInteractedAt { get; set; } = DateTime.UtcNow;
    }
}