using System.ComponentModel.DataAnnotations;

namespace Versum.Dtos
{
    public class BecomeAuthorDto
    {



        [Required(ErrorMessage = "Будь ласка, заповніть біографію автора")]

        [MinLength(10, ErrorMessage = "Мінімум 10 символів")]
        [MaxLength(500, ErrorMessage = "Максимум 500 символів")]
        public string AuthorBio { get; set; } = "";



    }
}
