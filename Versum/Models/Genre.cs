using System.ComponentModel.DataAnnotations;
using Versum;

public class Genre
{
    public int Id { get; set; }
    [MaxLength(50)] public string Name { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}