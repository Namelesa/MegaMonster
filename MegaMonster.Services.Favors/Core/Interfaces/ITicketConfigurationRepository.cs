using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface ITicketConfigurationRepository : IRepository<TicketConfiguration>
{
    Task<TicketConfiguration> GetConfigurationAsync(string userType);
}