
using System.ComponentModel.DataAnnotations;

namespace MovieStreaming.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MovieSlug { get; set; } = "";

    [Required]
    public string UserId { get; set; } = "";

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}
