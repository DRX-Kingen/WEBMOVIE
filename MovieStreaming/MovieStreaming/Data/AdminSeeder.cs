
using Microsoft.AspNetCore.Identity;

namespace MovieStreaming.Data;

public static class AdminSeeder
{
    public static async Task AssignAdminAsync(
        IServiceProvider services,
        string email)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.FindByEmailAsync(
            email.Trim()
        );

        if (user == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tài khoản: " + email
            );
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
            return;

        var result = await userManager.AddToRoleAsync(
            user,
            "Admin"
        );

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Không thể cấp quyền Admin: " +
                string.Join(", ",
                    result.Errors.Select(e => e.Description))
            );
        }
    }
}
