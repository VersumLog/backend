using Versum.Core.Enums;

namespace Versum.Dtos
{
    public class PostQueryDto
    {
        public FilterOptions Filter { get; set; }
        public bool Ascending { get; set; }
    }
}
