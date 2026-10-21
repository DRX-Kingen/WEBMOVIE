using System.Text.Json.Serialization;
namespace MovieStreaming.DTOs;
public class VsmovMovieDto
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? PosterUrl { get; set; }
    public string? ThumbUrl { get; set; }
    public int? Year { get; set; }
}