using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieStreaming.Data;
using MovieStreaming.Models;
namespace MovieStreaming.Controllers;

[ApiController]
[Route("api/movies")]
public class  MovieController: ControllerBase
{
    private readonly ApplicationDbContext db;
    public MovieController(ApplicationDbContext db)
    {
        this.db = db;
    }
    [HttpGet]
    public async Task<IActionResult> GetMovies(
        [FromQuery] string? keyword,
        [FromQuery] int page=1)
    {
        page = Math.Max(page, 1);
        var query = db.Movies.AsNoTracking().Where(m=>m.IsPublished&& !m.IsDeleted);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(m => m.Title.Contains(keyword));

        }
        var movies = await query.OrderByDescending(m => m.CreatedAt).Skip((page - 1) * 20).Take(20).ToListAsync();
        return Ok(movies);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await db.Movies.AsNoTracking().Include(m => m.Episodes).FirstOrDefaultAsync(m => m.Id == id && m.IsPublished && !m.IsDeleted);
        if (movie==null)
        {
            return NotFound();
        }
        return Ok(movie);
    }
    [HttpPost]
    public async Task<IActionResult> Create(Movie movie)
    {
        movie.Id = 0;
        movie.CreatedAt = DateTime.UtcNow;
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
    }

        
}
