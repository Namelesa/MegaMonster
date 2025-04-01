using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Core.Order;
using MegaMonster.Services.Card.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Card.Persistence.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await db.Orders.ToListAsync();
    }

    public async Task<bool> AddAsync(Order t)
    {
        try
        {
            await db.Orders.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> EditAsync(Order t)
    {
        try
        {
            db.Orders.Update(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Order t)
    {
        try
        {
            db.Orders.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<List<Guid>> GetOrdersIdByUserId(Guid userId)
    {
        var orders = await db.Orders.Where(t => t.UserId == userId).ToListAsync();
        return orders.Select(o => o.Id).ToList();
    }
    
    public async Task<List<int>> GetTicketsIdByUserId(Guid userId)
    {
        return await db.Orders
            .Where(o => o.UserId == userId)
            .SelectMany(o => o.OrderDetails)
            .Select(od => od.TicketId)    
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersUserId(Guid userId)
    {
        return await db.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order?> GetOrder(Guid id)
    {
        return await db.Orders.FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<Order?> GetAllOrderInfo(Guid orderId)
    {
        return await db.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}