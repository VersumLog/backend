using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Введіть логін або email")]
        [MaxLength(50, ErrorMessage = "Поле не може містити більше 50-ти символів")]
        public string UsernameOrGmail { get; set; } = string.Empty;


        [Required(ErrorMessage = "Введіть пароль")]
        [MaxLength(20, ErrorMessage = "Пароль не може містити більше 20 символів")]
        public string Password { get; set; } = string.Empty;
    }
}