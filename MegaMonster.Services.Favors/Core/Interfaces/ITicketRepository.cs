using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<List<Ticket>> GetTicketByStatus(string status);
    Task<Ticket> GetTicketById(int id);
}