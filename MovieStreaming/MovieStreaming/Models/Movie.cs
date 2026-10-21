using System.ComponentModel.DataAnnotations;
namespace MovieStreaming.Models;

public class Movie
{
    public int Id { get; set; }
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = " ";
    [Required]
    [MaxLength(255)]
    public string Slug { get; set; } = " ";
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public int? ReleaseYear { get; set; }
    public string? Country { get; set; }
    public string SourceProvider { get; set; } = "Local";
    public string ? Sourceslug { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

}
