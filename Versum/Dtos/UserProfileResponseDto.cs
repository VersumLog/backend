namespace Versum.Dtos
{
    public class UserProfileResponseDto
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsOwner { get; set; }
    }
}
