using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class DeletePhraseDto
    {
        [Required]
        public int Id { get; set; }
    }
}
