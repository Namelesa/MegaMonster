using MassTransit;
using MegaMonster.MessagingModels.Card;
using MegaMonster.Services.Card.Application.Order;
using Wc = MegaMonster.Services.Card.Core.Wc;

namespace MegaMonster.Services.Card.Application.Messaging;

public class CardCanceledStatusConsumer(OrderService orderService, ILogger<CardCanceledStatusConsumer> logger) : IConsumer<CardCanceledStatus>
{
    public async Task Consume(ConsumeContext<CardCanceledStatus> context)
    {
        var order = context.Message;
        
        try
        {
            logger.LogInformation($"Received payment update for OrderId: {order.OrderId}");

            var result = await orderService.UpdateOrderStatus(order.OrderId, null, Wc.CanceledStatus);

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