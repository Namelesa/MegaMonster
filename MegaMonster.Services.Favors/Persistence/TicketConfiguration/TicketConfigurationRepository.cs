using MegaMonster.Services.Favors.Core.TicketConfiguration;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.TicketConfiguration;

public class TicketConfigurationRepository(AppDbContext db) : ITicketConfigurationRepository
{
    public async Task<IEnumerable<Core.TicketConfiguration.TicketConfiguration>> GetAll()
    {
        return await db.Configurations.ToListAsync<Core.TicketConfiguration.TicketConfiguration>();
    }

    public async Task<bool> AddAsync(Core.TicketConfiguration.TicketConfiguration t)
    {
        try
        {
            await db.Configurations.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.TicketConfiguration.TicketConfiguration t)
    {
        try
        {
            db.Configurations.Update(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Core.TicketConfiguration.TicketConfiguration t)
    {
        try
        {
            db.Configurations.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<Core.TicketConfiguration.TicketConfiguration> GetConfigurationAsync(string userType)
    {
        var configuration = await db.Configurations.FirstOrDefaultAsync<Core.TicketConfiguration.TicketConfiguration>(c => c.UserType == userType);

        if (configuration == null)
        {
            throw new ArgumentException($"Configuration for {userType} not found.");
        }

        return configuration;
    }
}