using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class DeleteAccountDto
    {
        [Required(ErrorMessage = "Введіть свій пароль")]
        [StringLength(20, MinimumLength = 8,
             ErrorMessage = "Пароль має містити від 8 до 20 символів")]
        [RegularExpression(@"^\S+$",
             ErrorMessage = "Пароль не може містити пробіли")]
        public string Password { get; set; } = string.Empty;
    }
}
}
