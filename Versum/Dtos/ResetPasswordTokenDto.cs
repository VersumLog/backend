using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class ResetPasswordTokenDto
    {
        [Required(ErrorMessage = "Введіть код підтвердження")]
        [StringLength(6, ErrorMessage = "Код повинен складатися з 6 символів")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть email")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Неправильний email")]
        [MaxLength(50, ErrorMessage = "Поле email не може містити більше 50-ти символів")]
        public string Email { get; set; } = string.Empty;
    }
}
