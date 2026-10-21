
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieStreaming.Data;

namespace MovieStreaming.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var totalMovies = await _db.Movies.CountAsync();
        var totalEpisodes = await _db.Episodes.CountAsync();
        var totalComments = await _db.Comments.CountAsync();
        var totalUsers = await _db.Users.CountAsync();

        return Ok(new
        {
            totalMovies,
            totalEpisodes,
            totalComments,
            totalUsers
        });
    }
}
