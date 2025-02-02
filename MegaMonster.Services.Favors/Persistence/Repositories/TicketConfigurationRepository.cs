using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Repositories;

public class TicketConfigurationRepository(AppDbContext db) : ITicketConfigurationRepository
{
    public async Task<IEnumerable<TicketConfiguration>> GetAll()
    {
        return await db.Configurations.ToListAsync();
    }

    public async Task<bool> AddAsync(TicketConfiguration t)
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

    public async Task<bool> EditAsync(TicketConfiguration t)
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

    public async Task<bool> DeleteAsync(TicketConfiguration t)
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
    
    public async Task<TicketConfiguration> GetConfigurationAsync(string userType)
    {
        var configuration = await db.Configurations.FirstOrDefaultAsync(c => c.UserType == userType);

        if (configuration == null)
        {
            throw new ArgumentException($"Configuration for {userType} not found.");
        }

        return configuration;
    }
}