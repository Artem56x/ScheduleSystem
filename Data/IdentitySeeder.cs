using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScheduleSystem.Models;

namespace ScheduleSystem.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var roleResult =
                await roleManager.CreateAsync(new IdentityRole(role));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось создать роль '{role}': {errors}");
            }
        }

        const string adminEmail = "admin@schedulesystem.local";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            var configuration =
                serviceProvider.GetRequiredService<IConfiguration>();

            var adminPassword = configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Set the admin password using the Admin:Password " +
                    "configuration key or the Admin__Password environment variable.");
            }

            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(admin, adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось создать администратора: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult =
                await userManager.AddToRoleAsync(admin, "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось назначить администратору роль Admin: {errors}");
            }
        }
    }
}