namespace MegaMonster.Services.Payment.Core.Payment;

public interface IPaymentRepository
{
    Task<bool> AddPaymentAsync(Payments payments);
    Task<Payments?> GetPaymentAsyncByOrderId(Guid orderId);
    Task<bool> UpdatePaymentAsync(Payments payments);
}