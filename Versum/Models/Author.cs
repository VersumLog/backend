using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Versum.Models
{
    public class Author
    {


        [Key, ForeignKey("User")]
        public int AuthorId { get; set; }  

        public string? AuthorBio { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
