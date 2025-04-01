using MegaMonster.Services.Payment.Core.Payment;
using MegaMonster.Services.Payment.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Payment.Persistence.Repositories;

public class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public async Task<bool> AddPaymentAsync(Payments payments)
    {
        try
        {
            await db.Payments.AddAsync(payments);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    
    public async Task<Payments?> GetPaymentAsyncByOrderId(Guid orderId)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(u => u.OrderId == orderId);
        return payment;
    }

    public async Task<bool> UpdatePaymentAsync(Payments payments)
    {
        try
        {
            db.Payments.Update(payments);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}