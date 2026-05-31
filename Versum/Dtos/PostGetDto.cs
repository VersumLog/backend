using System.ComponentModel.DataAnnotations;
using System.Data;


namespace Versum.Dtos
{
    public class PostGetDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<string> Genres { get; set; } = new List<string>();
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public bool IsSavedByUser { get; set; }

    }
}