using MegaMonster.Services.Favors.Core.TicketConfiguration;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Data.DbInitializer;

public class DbInitializer(AppDbContext db) : IDbInitializer
{
    public async Task Initialize()
    {
        try
        {
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
            {
                await db.Database.MigrateAsync();
                await db.SaveChangesAsync();
            }
        }
        catch
        {
            Console.WriteLine("Can not do migration");
        }

        await SeedConfigsAsync();
    }

    private async Task SeedConfigsAsync()
    {
        if (await db.Configurations.AnyAsync()) return;

        var configurations = new List<Core.TicketConfiguration.TicketConfiguration>
        {
            new()
            {
                UserType = "Student",
                Price = 20,
                DurationInHours = 3
            },
            new()
            {
                UserType = "Adult",
                Price = 30,
                DurationInHours = 4
            },
            new()
            {
                UserType = "Baby",
                Price = 10,
                DurationInHours = 4
            }
        };

        await db.Configurations.AddRangeAsync(configurations);
        await db.SaveChangesAsync();
        Console.WriteLine("Configurations added");
    }
}