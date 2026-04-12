namespace Versum
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = "NotAsasha";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
