using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Persistence.DbInitializer;

public class DbInitializer(AppDbContext db) : IDbInitializer
{
    public async Task Initialize()
    {
        try
        {
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
            {
                await db.Database.MigrateAsync();
            }
        }
        catch
        {
            return;
        }
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (await db.Roles.AnyAsync()) return;

        var roles = new List<Role>
        {
            new("Admin"),
            new("Customer")
        };

        await db.Roles.AddRangeAsync(roles);
        await db.SaveChangesAsync();
        Console.WriteLine("Roles added");
    }
}