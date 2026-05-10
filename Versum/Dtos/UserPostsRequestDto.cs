using System.ComponentModel.DataAnnotations;
using Versum.Core.Enums;

namespace Versum.Dtos
{
    public class UserPostsRequestDto
    {
        [Required(ErrorMessage = "Введіть свій нікнейм")]
        public string Username { get; set; } = string.Empty;
        public FilterOptions Filter { get; set; }
        public bool Ascending { get; set; }

    }
}