namespace Versum.Dtos
{
    public class DictResponceDto
    {
        public int Id { get; set; }
        public int? PostId { get; set; }
        public string Phrase { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AnchorId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
