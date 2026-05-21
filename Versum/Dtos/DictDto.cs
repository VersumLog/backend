using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class DictDto
    {
        [Required]
        [MaxLength(200)] public string Phrase { get; set; } = string.Empty;

        [Required]
        [MaxLength(600)] public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(600)] public string AnchorId { get; set; } = string.Empty;

    }
}
