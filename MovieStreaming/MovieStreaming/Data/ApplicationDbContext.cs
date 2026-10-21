using MovieStreaming.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace MovieStreaming.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
       DbContextOptions<ApplicationDbContext> options
   ) : base(options)
        {
        }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<WatchEvent> WatchEvents => Set<WatchEvent>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Movie>()
                .HasIndex(m => m.Slug)
                .IsUnique();

            builder.Entity<Rating>()
                .HasIndex(r => new { r.MovieId, r.UserId })
                .IsUnique();

            builder.Entity<Rating>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Ratings_Score",
                    "[Score] BETWEEN 1 AND 5"
                ));

            builder.Entity<Episode>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Episodes)
                .HasForeignKey(e => e.MovieId);
        }
    }
}
