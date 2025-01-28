using MegaMonster.Services.Favors.Data;
namespace MegaMonster.Services.Favors.Service;

using Microsoft.EntityFrameworkCore;

public class TicketService(AppDbContext dbContext)
{
    public async Task<Dictionary<string, TicketConfiguration>> LoadConfigurationsAsync()
    {
        var configurations = await dbContext.Set<TicketConfiguration>().ToListAsync();
        
        var result = new Dictionary<string, TicketConfiguration>();
        foreach (var config in configurations)
        {
            result[config.UserType.ToString()] = config;
        }

        return result;
    }

    public async Task<TicketConfiguration> GetConfigurationAsync(string userType)
    {
        var configuration = await dbContext.Set<TicketConfiguration>()
            .FirstOrDefaultAsync(c => c.UserType == userType);

        if (configuration == null)
        {
            throw new ArgumentException($"Configuration for {userType} not found.");
        }

        return configuration;
    }
}
