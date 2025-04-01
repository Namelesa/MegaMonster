using MegaMonster.Services.Favors.Core.Interfaces;

namespace MegaMonster.Services.Favors.Core.Ticket;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<List<Ticket>> GetTicketByStatus(string status);
    Task<Ticket> GetTicketById(int id);
}