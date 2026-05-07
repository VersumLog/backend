using Versum.Core.Enums;

namespace Versum.Dtos
{
    public class UserPostsRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public FilterOptions Filter { get; set; }
        public bool Ascending { get; set; }

    }
}