using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Versum.Models
{
    public class Author
    {


        [Key, ForeignKey("User")]
        public int AuthorId { get; set; }

        [MaxLength(500)] public string? AuthorBio { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
