using System.ComponentModel.DataAnnotations;


namespace Versum.Dtos
{
    public class PostDto
    {
        [Required(ErrorMessage = "Введіть назву твору")]
        [MaxLength(100, ErrorMessage = "Максимум 100 символів")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Введіть опис твору")]
        [MaxLength(600, ErrorMessage = "Максимум 600 символів")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Твір не може бути порожнім")]
        [MaxLength(500000, ErrorMessage = "Максимум 500000 символів")]
        [MinLength(10, ErrorMessage = "Мінімум 10 символів")]
        public string Content { get; set; } = "";



    }
}