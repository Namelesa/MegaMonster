using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Repositories;

public class TicketRepository(AppDbContext db) : ITicketRepository
{
    public async Task<IEnumerable<Ticket>> GetAll()
    {
        return await db.Tickets.ToListAsync();
    }

    public async Task<bool> AddAsync(Ticket t)
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

    public async Task<bool> EditAsync(Ticket t)
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

    public async Task<bool> DeleteAsync(Ticket t)
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
    
    public async Task<List<Ticket>> GetTicketByStatus(string status)
    {
        var tickets = await db.Tickets
            .Where(t => t.UserType == status)
            .ToListAsync();
        return tickets;
    }
    
    public async Task<Ticket> GetTicketById(int id)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(u => u.Id == id);
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket with id = {id} not found");
        }

        return ticket;
    }
}