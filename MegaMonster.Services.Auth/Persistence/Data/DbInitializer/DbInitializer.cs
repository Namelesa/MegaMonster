using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Data.DbInitializer;

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
            Console.WriteLine("Can not do migration");
        }
    }
}