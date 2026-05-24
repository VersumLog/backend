using System.ComponentModel.DataAnnotations;

namespace Versum.Models
{
    public class Dictionary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? PostId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Post? Post { get; set; } = null!;

        [MaxLength(200)] public string Phrase { get; set; } = string.Empty;
        [MaxLength(600)] public string Description { get; set; } = string.Empty;
        [MaxLength(60)] public string AnchorId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

    }
}
