using MegaMonster.Services.Payment.Core.Payment;

namespace MegaMonster.Services.Payment.Application.Payment;

public class PaymentServiceRepository(IPaymentRepository paymentRepository)
{
    public async Task<OperationResult<Payments>> GetPaymentByOrderId(Guid orderId)
    {
        var payment = await paymentRepository.GetPaymentAsyncByOrderId(orderId);
        if (payment == null) return OperationResult<Payments>.Fail("Not found payment for this order");

        return OperationResult<Payments>.Ok(payment);
    }
    
    public async Task<OperationResult<string>> AddPayment(Payments payment)
    {
        var result = await paymentRepository.AddPaymentAsync(payment);
        return result
            ? OperationResult<string>.Ok("Add new payment")
            : OperationResult<string>.Fail("Can not add this payment");
    }
    
    public async Task<OperationResult<string>> UpdatePayment(Payments payment)
    {
        var result = await paymentRepository.UpdatePaymentAsync(payment);
        return result
            ? OperationResult<string>.Ok("update payment")
            : OperationResult<string>.Fail("Can not update this payment");
    }
}