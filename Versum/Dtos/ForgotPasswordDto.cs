using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Введіть email")]
        [MaxLength(50, ErrorMessage = "Поле не може містити більше 50-ти символів")]
        public string Email { get; set; } = string.Empty;
    }
}
