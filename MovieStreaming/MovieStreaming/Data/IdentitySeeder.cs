
using Microsoft.AspNetCore.Identity;

namespace MovieStreaming.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "User" };

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName)
            );

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Không thể tạo role {roleName}: " +
                    string.Join(", ",
                        result.Errors.Select(e => e.Description))
                );
            }
        }
    }
}
