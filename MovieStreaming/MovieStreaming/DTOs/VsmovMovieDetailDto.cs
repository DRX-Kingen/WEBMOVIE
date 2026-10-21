
namespace MovieStreaming.DTOs;

public class VsmovMovieDetailDto
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? OriginName { get; set; }
    public string? Content { get; set; }
    public string? PosterUrl { get; set; }
    public string? ThumbUrl { get; set; }
    public int? Year { get; set; }
    public string? Time { get; set; }
    public string? EpisodeCurrent { get; set; }

    public List<string> Categories { get; set; } = new();
    public List<string> Countries { get; set; } = new();
    public List<VsmovEpisodeServerDto> Episodes { get; set; } = new();
}

public class VsmovEpisodeServerDto
{
    public string ServerName { get; set; } = "";
    public List<VsmovEpisodeDto> Items { get; set; } = new();
}

public class VsmovEpisodeDto
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? LinkEmbed { get; set; }
}
