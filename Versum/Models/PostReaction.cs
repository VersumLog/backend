using System;
using System.ComponentModel.DataAnnotations;

namespace Versum.Models
{
    public enum ReactionType
    {
        View = 0,  // Пост просто з'явився у стрічці користувача
        Like = 1,  // Користувач лайкнув пост
        Share = 2  // Користувач поділився постом
    }

    public class PostReaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        public ReactionType Type { get; set; }
        public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
    }
}