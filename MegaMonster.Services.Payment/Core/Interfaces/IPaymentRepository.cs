using MegaMonster.Services.Payment.Core.Models;

namespace MegaMonster.Services.Payment.Core.Interfaces;

public interface IPaymentRepository
{
    Task<bool> AddPaymentAsync(Payments payments);
    Task<Payments?> GetPaymentAsyncByOrderId(Guid orderId);
    Task<bool> UpdatePaymentAsync(Payments payments);
}