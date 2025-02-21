using MassTransit;
using MegaMonster.Services.Payment.Infrastructure.Service;
using MegaMonster.MessagingModels.InfoPayment;

namespace MegaMonster.Services.Payment.Infrastructure.Messaging
{
    public class PaymentConsumer(PaymentService paymentService, ILogger<PaymentConsumer> logger)
        : IConsumer<InfoPaymentList>
    {
        public async Task Consume(ConsumeContext<InfoPaymentList> context)
        {
            var paymentInfoList = context.Message.Payments;
            
            foreach (var paymentInfo in paymentInfoList)
            {
                try
                {
                    var result = await paymentService.CreatePaymentAsync(
                        paymentInfo.OrderId,
                        paymentInfo.UserName,
                        paymentInfo.Sum,
                        paymentInfo.Count,
                        paymentInfo.Action
                    );

                    if (!string.IsNullOrEmpty(result))
                    {
                        logger.LogInformation($"Payment URL generated for Order ID {paymentInfo.OrderId}: {result}");
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
}
