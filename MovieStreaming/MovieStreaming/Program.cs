
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieStreaming.Data;
using MovieStreaming.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================================
// 1. Controllers
// =======================================
builder.Services.AddControllers();

// =======================================
// 2. Database MovieStreaming + Identity
// =======================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

// =======================================
// 3. ASP.NET Core Identity
// =======================================
builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // Hỗ trợ khóa tài khoản
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =======================================
// 4. CORS cho React
// =======================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =======================================
// 5. HttpClient VSMOV
// =======================================
builder.Services.AddHttpClient<VsmovService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Vsmov:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Thiếu Vsmov:BaseUrl"
        )
    );

    client.Timeout = TimeSpan.FromSeconds(20);
});

// =======================================
// 6. JWT Configuration
// =======================================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Thiếu Jwt:Key"
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Thiếu Jwt:Issuer"
    );

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Thiếu Jwt:Audience"
    );

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key phải có tối thiểu 32 byte UTF-8."
    );
}

// =======================================
// 7. JWT Authentication
// =======================================
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                // Nhận diện tên và quyền
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,

                ClockSkew = TimeSpan.Zero
            };
    });

// =======================================
// 8. Authorization
// =======================================
builder.Services.AddAuthorization();

// =======================================
// 9. Build
// =======================================
var app = builder.Build();

// =======================================
// 10. Middleware
// =======================================
app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

// =======================================
// 11. API Controllers
// =======================================
app.MapControllers();

// =======================================
// 12. Khởi tạo roles
// =======================================
await IdentitySeeder.SeedRolesAsync(app.Services);

// =======================================
// 13. Cấp Admin lần đầu (Development)
// =======================================
if (app.Environment.IsDevelopment())
{
    var adminEmail =
        app.Configuration["BootstrapAdmin:Email"];

    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        await AdminSeeder.AssignAdminAsync(
            app.Services,
            adminEmail
        );
    }
}

// =======================================
// 14. Run
// =======================================
app.Run();
