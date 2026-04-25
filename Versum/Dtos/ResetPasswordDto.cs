using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Введіть код підтвердження")]
        [StringLength(6, ErrorMessage = "Код повинен складатися з 6 символів")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть новий пароль")]
        [StringLength(20, MinimumLength = 8,
            ErrorMessage = "Пароль має містити від 8 до 20 символів")]
        [RegularExpression(@"^\S+$",
            ErrorMessage = "Пароль не може містити пробіли")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть email")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Неправильний email")] // checks if gmail is in right form
        [MaxLength(50, ErrorMessage = "поле email неможе містити більше 50-ти символів")]
        public string Email { get; set; } = string.Empty;
    }
}
