using System.ComponentModel.DataAnnotations;


namespace Versum.Dtos
{
    public class PostDto
    {
       
        [MaxLength(100, ErrorMessage = "Максимум 100 символів")]
        public string Title { get; set; } = "";

       
        [MaxLength(600, ErrorMessage = "Максимум 600 символів")]
        public string Description { get; set; } = "";

      
        [MaxLength(500000, ErrorMessage = "Максимум 500000 символів")]
        [MinLength(10, ErrorMessage = "Мінімум 10 символів")]
        public string Content { get; set; } = "";



    }
}