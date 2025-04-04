using MegaMonster.Services.Favors.Core.BaseModel;

namespace MegaMonster.Services.Favors.Core.TicketConfiguration;

public interface ITicketConfigurationRepository : IRepository<TicketConfiguration>
{
    Task<TicketConfiguration> GetConfigurationAsync(string userType);
}