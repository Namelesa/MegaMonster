using MassTransit;
using MegaMonster.MessagingModels.Payment.Card;
using MegaMonster.Services.Card.Application.Order;

namespace MegaMonster.Services.Card.Application.Messaging;

public class CardInfoStatusConsumer(OrderService orderService, ILogger<CardInfoStatusConsumer> logger) : IConsumer<InfoForCardPayment>
{
    public async Task Consume(ConsumeContext<InfoForCardPayment> context)
    {
        var order = context.Message;
        
        try
        {
            logger.LogInformation($"Received payment update for OrderId: {order.OrderId}");

            var result = await orderService.UpdateOrderStatus(order.OrderId, order.Bill);

            if (result.Success)
            {
                logger.LogInformation($"Order {order.OrderId} status updated successfully.");
            }
            else
            {
                logger.LogWarning($"Failed to update Order {order.OrderId}: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error processing payment update for OrderId: {order.OrderId}");
            throw;
        }
    }
}