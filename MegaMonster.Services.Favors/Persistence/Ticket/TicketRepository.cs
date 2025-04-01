using MegaMonster.Services.Favors.Core.Ticket;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Ticket;

public class TicketRepository(AppDbContext db) : ITicketRepository
{
    public async Task<IEnumerable<Core.Ticket.Ticket>> GetAll()
    {
        return await db.Tickets.ToListAsync<Core.Ticket.Ticket>();
    }

    public async Task<bool> AddAsync(Core.Ticket.Ticket t)
    {
        try
        {
            await db.Tickets.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.Ticket.Ticket t)
    {
        try
        {
            db.Tickets.Update(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Core.Ticket.Ticket t)
    {
        try
        {
            db.Tickets.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<List<Core.Ticket.Ticket>> GetTicketByStatus(string status)
    {
        var tickets = await Queryable
            .Where<Core.Ticket.Ticket>(db.Tickets, t => t.UserType == status)
            .ToListAsync();
        return tickets;
    }
    
    public async Task<Core.Ticket.Ticket> GetTicketById(int id)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with id = {id} not found");
        }

        return ticket;
    }
}