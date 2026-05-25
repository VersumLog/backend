namespace Versum.Models
{
    public class Savings
    {

        public int UserId { get; set; }
        public int PostId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
