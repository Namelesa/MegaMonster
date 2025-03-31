using MassTransit;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.Models;
using MegaMonster.MessagingModels.UserNotification;

namespace MegaMonster.Services.Notification.Application.Messaging;

public class UserBanConsumer(INotification notificationService) : IConsumer<UserBan>
{
    public async Task Consume(ConsumeContext<UserBan> context)
    {
        Console.WriteLine($"Received Message ID: {context.MessageId}");
        Console.WriteLine($"Register with name {context.Message.UserName} was notify");

        var userDto = new UserDto(context.Message.UserName, context.Message.Email);
        await notificationService.SendBanEmailAsync(userDto, context.Message.Reason);
    }
}