namespace Versum.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int TargetUserId { get; set; } 
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActorUsername { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
