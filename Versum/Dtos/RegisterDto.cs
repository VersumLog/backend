
using System.ComponentModel.DataAnnotations;


namespace Versum.Dtos{
    public class RegisterDto
    {

        [Required(ErrorMessage = "Введіть свій нікнейм")] // Field can't be null
        [RegularExpression(@"^[a-z0-9_]+$",
        ErrorMessage = "Нікнейм може містити лише цифри, малі літери та підкреслення")] // Allowed symbols: only lowercase (a-z), numbers (0-9), underscores (_)
        [MaxLength( 50, ErrorMessage = "поле нікнейму неможе містити більше 50-ти символів")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Введіть свій пароль")]
        [StringLength(20, MinimumLength = 8,
            ErrorMessage = "Пароль має містити від 8 до 20 символів")]
        [RegularExpression(@"^\S+$",
            ErrorMessage = "Пароль не може містити пробіли")]
       
        public string Password { get; set; } = "";
       

        [Required(ErrorMessage = "Введіть email")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Неправильний email")] // checks if gmail is in right form
        [MaxLength(50, ErrorMessage = "поле email неможе містити більше 50-ти символів")]
        public string Email { get; set; } = string.Empty;
    }
}
