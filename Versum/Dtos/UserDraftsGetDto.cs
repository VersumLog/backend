using System.ComponentModel.DataAnnotations;
using System.Data;


namespace Versum.Dtos
{
    public class UserDraftsGetDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    }
}