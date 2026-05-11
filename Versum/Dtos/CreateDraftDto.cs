using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class CreateDraftDto
    {

        [Required(ErrorMessage = "Введіть назву твору")]
        [MaxLength(100, ErrorMessage = "Максимум 100 символів")]
        public string Title { get; set; } = "";
    }
}
