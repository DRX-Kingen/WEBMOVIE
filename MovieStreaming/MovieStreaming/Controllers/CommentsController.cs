
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieStreaming.Data;
using MovieStreaming.DTOs;
using MovieStreaming.Models;

namespace MovieStreaming.Controllers;

[ApiController]
[Route("api/movies/{movieSlug}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public CommentsController(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: api/movies/naruto/comments?page=1
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(
        string movieSlug,
        int page = 1)
    {
        page = Math.Clamp(page, 1, 100000);
        const int pageSize = 10;

        var query = _db.Comments
            .AsNoTracking()
            .Where(c => c.MovieSlug == movieSlug);

        var total = await query.CountAsync();

        var comments = await (
            from c in query
            join u in _db.Users on c.UserId equals u.Id
            orderby c.CreatedAt descending, c.Id descending
            select new CommentResponseDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = u.UserName ?? "Người dùng",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }
        )
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

        return Ok(new
        {
            items = comments,
            total,
            page,
            pageSize
        });
    }

    // POST: api/movies/naruto/comments
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment(
        string movieSlug,
        [FromBody] CreateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(movieSlug)
            || movieSlug.Length > 100)
            return BadRequest("MovieSlug không hợp lệ.");

        var content = request.Content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("Nội dung không được để trống.");

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var comment = new Comment
        {
            MovieSlug = movieSlug,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(userId);

        return Ok(new CommentResponseDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = user?.UserName ?? "Người dùng",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        });
    }

    // DELETE: api/movies/naruto/comments/12
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(
        string movieSlug,
        int id)
    {
        var comment = await _db.Comments
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.MovieSlug == movieSlug);

        if (comment == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        if (comment.UserId != userId && !isAdmin)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
