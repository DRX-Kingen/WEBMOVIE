
using System.ComponentModel.DataAnnotations;

namespace MovieStreaming.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; } = "";
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
