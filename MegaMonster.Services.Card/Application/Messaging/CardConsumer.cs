using MassTransit;
using MegaMonster.MessagingModels.Card;
using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Application.Order;
using MegaMonster.Services.Card.Core.OrderDetail;

namespace MegaMonster.Services.Card.Application.Messaging;

public class CardConsumer(OrderService orderService) : IConsumer<CardInfoModel>
{
    public async Task Consume(ConsumeContext<CardInfoModel> context)
    {
        var card = context.Message;
        var orderDetailsList = card.TicketDetails.Select(ticket => new OrderDetails()
        {
            TicketId = ticket.TicketId,
        }).ToList();

        var order = new Core.Order.Order()
        {
            UserId = card.UserId,
            UserName = card.UserName,
            Sum = card.Sum,
            OrderDetails = orderDetailsList,
            PaymentType = card.PaymentType
        };
        
        await orderService.AddOrder(order);
    }
}