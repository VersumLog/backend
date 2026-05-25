namespace Versum.Dtos
{
    public class CommentGetDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; }
    }
}
