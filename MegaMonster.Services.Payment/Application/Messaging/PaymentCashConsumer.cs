using MassTransit;
using MegaMonster.MessagingModels.InfoPayment.CashPaymentModels;
using MegaMonster.Services.Payment.Infrastructure.Paymenet;

namespace MegaMonster.Services.Payment.Application.Messaging;

public class PaymentCashConsumer(PaymentService paymentService, ILogger<PaymentConsumer> logger) : IConsumer<InfoPaymentListCash>
{
    public async Task Consume(ConsumeContext<InfoPaymentListCash> context)
    {
        var paymentInfoList = context.Message.Payments;
        Console.WriteLine($"Input data for cash  {paymentInfoList.Count}");
        foreach (var paymentInfo in paymentInfoList)
        {
            try
            {
                var result = await paymentService.AddCardPaymentsAsync(
                    paymentInfo.OrderId,
                    paymentInfo.UserName,
                    paymentInfo.Sum,
                    paymentInfo.Count
                );

                if (result)
                {
                    logger.LogInformation($"Payment generated for Order ID {paymentInfo.OrderId}");
                }
                else
                {
                    logger.LogError($"Failed to create payment for Order ID {paymentInfo.OrderId}: invalid result");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception occurred while processing payment for Order ID {paymentInfo.OrderId}: {ex.Message}");
            }
        }
    }
}