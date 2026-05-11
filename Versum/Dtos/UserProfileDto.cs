using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class UserProfileDto
    {
        [Required(ErrorMessage = "Введіть свій нікнейм")] // Field can't be null
        [RegularExpression(@"^[a-z0-9_]+$",
        ErrorMessage = "Нікнейм може містити лише цифри, малі літери та підкреслення")] // Allowed symbols: only lowercase (a-z), numbers (0-9), underscores (_)
        [MaxLength(50, ErrorMessage = "Поле нікнейму не може містити більше 50-ти символів")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть своє ім'я")]
        [MaxLength(30, ErrorMessage = "Поле імені не може містити більше 30-ти символів")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть свою біографію")]
        [MaxLength(200, ErrorMessage = "Поле біографії не може містити більше 200-ти символів")]
        public string Bio { get; set; } = string.Empty;


    }
}