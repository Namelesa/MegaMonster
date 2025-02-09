using MassTransit;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.Models;
using MessagingModels.UserNotification;

namespace MegaMonster.Services.Notification.Application.Messaging;

public class NotifyUserConsumer(INotification notificationService) : IConsumer<UserNotificationBase>
{
    public async Task Consume(ConsumeContext<UserNotificationBase> context)
    {
        Console.WriteLine($"Received Message ID: {context.MessageId}");
        Console.WriteLine($"User with name {context.Message.UserName} was notify");

        var userDto = new UserDto(context.Message.UserName, context.Message.Email);
        await notificationService.SendConfirmEmailAsync(userDto);
    }
}