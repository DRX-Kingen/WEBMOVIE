namespace MovieStreaming.Models
{
    public class WatchEvent
    {
        public long Id { get; set; }

        public int MovieId { get; set; }

        public int? EpisodeId { get; set; }

        public string? UserId { get; set; }

        public string SessionId { get; set; } = "";

        public DateTime WatchedAt { get; set; }
            = DateTime.UtcNow;
    }
}
