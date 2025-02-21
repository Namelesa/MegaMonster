using MassTransit;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.Models;
using MegaMonster.MessagingModels.InfoPayment;

namespace MegaMonster.Services.Notification.Application.Messaging;

public class NotifyUserBillConsumer(INotification notificationService) : IConsumer<InfoBillModel>
{
    public async Task Consume(ConsumeContext<InfoBillModel> context)
    {
        var paymentBill = context.Message;

        var userDto = new BillUserDto()
        {
            UserName = paymentBill.UserName,
            Email = paymentBill.Email,
            OrderId = paymentBill.OrderId,
            PaymentType = paymentBill.PaymentType,
            Status = paymentBill.PaymentStatus,
            Sum = paymentBill.Sum,
            OrderDetailsRows = paymentBill.TicketsIds
        };
        
        await notificationService.SendBillEmailAsync(userDto);
    }
}