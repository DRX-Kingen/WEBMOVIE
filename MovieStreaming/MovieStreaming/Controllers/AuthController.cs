
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieStreaming.Data;
using MovieStreaming.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieStreaming.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _config = config;
    }

  
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                message = "Dữ liệu đăng ký không hợp lệ."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập đầy đủ thông tin."
            });
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest(new
            {
                message = "Mật khẩu xác nhận không khớp."
            });
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        
        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            return Conflict(new
            {
                message = "Email đã được đăng ký."
            });
        }

       
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            return StatusCode(500, new
            {
                message = "Hệ thống chưa khởi tạo quyền User."
            });
        }

       
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

       
        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var createResult =
            await _userManager.CreateAsync(
                user,
                dto.Password
            );

        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync();

            return BadRequest(new
            {
                message = "Đăng ký thất bại.",
                errors = createResult.Errors
                    .Select(e => e.Description)
            });
        }

       
        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "User"
            );

        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Không thể cấp quyền User.",
                errors = roleResult.Errors
                    .Select(e => e.Description)
            });
        }

        await transaction.CommitAsync();

        return StatusCode(201, new
        {
            message = "Đăng ký thành công."
        });
    }

    // =====================================
    // POST: api/auth/login
    // Đăng nhập và tạo JWT
    // =====================================
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        if (dto == null ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập email và mật khẩu."
            });
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        // Tìm tài khoản theo email
        var user =
            await _userManager.FindByEmailAsync(email);

        // Kiểm tra mật khẩu
        if (user == null ||
            !await _userManager.CheckPasswordAsync(
                user,
                dto.Password))
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không chính xác."
            });
        }

        // Kiểm tra tài khoản có bị khóa hay không
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new
            {
                message = "Tài khoản đang bị khóa."
            });
        }

        // Lấy danh sách quyền
        var roles =
            await _userManager.GetRolesAsync(user);

        // JWT hết hạn sau 2 giờ
        var expiresAt =
            DateTime.UtcNow.AddHours(2);

        // Claims chứa thông tin tài khoản
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id
            ),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? ""
            ),

            new(
                ClaimTypes.NameIdentifier,
                user.Id
            ),

            new(
                ClaimTypes.Name,
                user.UserName ?? ""
            ),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };

        // Thêm quyền Admin/User vào JWT
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role)
            );
        }

        // Lấy cấu hình JWT
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Thiếu Jwt:Key"
            );

        var jwtIssuer = _config["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Thiếu Jwt:Issuer"
            );

        var jwtAudience = _config["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Thiếu Jwt:Audience"
            );

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // Tạo JWT
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            expiresAt = expiresAt,
            userId = user.Id,

           
            fullName = user.UserName,

            email = user.Email,
            roles = roles
        });
    }

  
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "Bạn chưa đăng nhập."
            });
        }

        var user =
            await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Tài khoản không tồn tại."
            });
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            email = user.Email,
            roles = roles
        });
    }
}
