using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class DictDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(200)] public string Phrase { get; set; } = string.Empty;

        [Required]
        [MaxLength(600)] public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)] public string AnchorId { get; set; } = string.Empty;

    }
}
