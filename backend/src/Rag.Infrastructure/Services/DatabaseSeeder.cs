using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rag.Domain.Entities;

namespace Rag.Infrastructure.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(new Role { Name = "Admin" }, new Role { Name = "User" });
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync(x => x.Email == "admin@ragplatform.com"))
        {
            var adminRole = await db.Roles.FirstAsync(x => x.Name == "Admin");
            var admin = new User
            {
                Email = "admin@ragplatform.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Admin",
                IsActive = true
            };

            db.Users.Add(admin);
            db.UserRoles.Add(new UserRole { User = admin, RoleId = adminRole.Id });
            await db.SaveChangesAsync();
            logger.LogInformation("Default admin user seeded");
        }
    }
}
