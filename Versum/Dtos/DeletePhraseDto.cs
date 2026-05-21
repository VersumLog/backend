using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class DeletePhraseDto
    {


        [Required]
        [MaxLength(200)] public string Phrase { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }

    }
}
