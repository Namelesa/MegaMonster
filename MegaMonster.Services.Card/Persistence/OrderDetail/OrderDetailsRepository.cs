using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Core.OrderDetail;
using MegaMonster.Services.Card.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Card.Persistence.Repositories;

public class OrderDetailsRepository(AppDbContext db) : IOrderDetailsRepository
{
    public async Task<IEnumerable<OrderDetails>> GetAllAsync()
    {
        return await db.OrdersDetails.ToListAsync();
    }

    public async Task<bool> AddAsync(OrderDetails t)
    {
        try
        {
            await db.OrdersDetails.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> EditAsync(OrderDetails t)
    {
        try
        { 
            db.OrdersDetails.Update(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(OrderDetails t)
    {
        try
        {
            db.OrdersDetails.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<IEnumerable<OrderDetails>> GetDetailsForOrder(List<Guid> orderIds)
    {
        return await db.OrdersDetails.Where(od => orderIds.Contains(od.OrderId)).ToListAsync();
    }
}