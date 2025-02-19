using MassTransit;
using MegaMonster.Services.Auth.Application.Services;
using MessagingModels.UserNotification;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class BanUserConsumer(AuthService authService) : IConsumer<UserBanForAuth>
{
    public async Task Consume(ConsumeContext<UserBanForAuth> context)
    {
        var message = context.Message;

        await authService.BanUser(message.Email);
        Console.WriteLine("Baned user");
    }
}