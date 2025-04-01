using MassTransit;
using MegaMonster.MessagingModels.User.Notification;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.User;

namespace MegaMonster.Services.Notification.Application.Messaging;

public class NotifyUserConsumer(INotification notificationService) : IConsumer<UserNotificationBase>
{
    public async Task Consume(ConsumeContext<UserNotificationBase> context)
    {
        Console.WriteLine($"Received Message ID: {context.MessageId}");
        Console.WriteLine($"Register with name {context.Message.UserName} was notify");

        var userDto = new UserDto(context.Message.UserName, context.Message.Email, context.Message.ConfirmationLink);
        await notificationService.SendConfirmEmailAsync(userDto);
    }
}