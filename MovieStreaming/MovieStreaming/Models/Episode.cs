namespace MovieStreaming.Models
{
    public class Episode
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public string Title { get; set; } = "";
        public int EpisodeNumber { get; set; }

        public string? VideoUrl { get; set; }
        public string? VideoType { get; set; }
    }
}
