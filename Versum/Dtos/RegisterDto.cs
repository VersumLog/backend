
using System.ComponentModel.DataAnnotations;


namespace Versum.Dtos{
    public class RegisterDto
    {

        [Required(ErrorMessage = "Введіть свій нікнейм")] // Field can'n be null
        [RegularExpression(@"^[a-z0-9_]+$",
        ErrorMessage = "Нікнейм може містити лише цифри, малі літери та підкреслення")] // Allowed symbols: only lowercase (a-z), numbers (0-9), underscores (_)
        [MaxLength( 50, ErrorMessage = "поле нікнейму неможе містити більше 50-ти символів")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Введіть свій пароль")]
        [StringLength(8, MinimumLength = 8,
            ErrorMessage = "Пароль має містити рівно 8 символів")]
        [RegularExpression(@"^\S{8}$",
            ErrorMessage = "Пароль не може містити пробіли")]
       
        public string Password { get; set; } = "";
       

        [Required(ErrorMessage = "Введіть свій пароль ще раз")]
        [Compare("Password", ErrorMessage = "Ваш пароль не збігається")]
        public string ConfirmPassword { get; set; } = "";  // field for re-entry password


        [Required(ErrorMessage = "Введіть gmail")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$",
            ErrorMessage = "Неправильний gmail")] // checks if gmail is in right form
        [MaxLength(50, ErrorMessage = "поле gmail неможе містити більше 50-ти символів")]
        public string Gmail { get; set; } = string.Empty;
    }
}
