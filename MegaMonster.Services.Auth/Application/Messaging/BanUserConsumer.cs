using MassTransit;
using MegaMonster.MessagingModels.RollBacks.User;
using MegaMonster.MessagingModels.UserNotification;
using MegaMonster.Services.Auth.Application.User;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class BanUserConsumer(AuthService authService, IPublishEndpoint publishEndpoint) : IConsumer<UserBanForAuth>
{
    public async Task Consume(ConsumeContext<UserBanForAuth> context)
    {
        var message = context.Message;

        var result = await authService.BanUser(message.Email);

        if (!result.Success)
        {
            var user = await authService.FindUserByEmail(message.Email);
            
            if(user == null) return;

            var banUserRollback = new BanUserRollBack
            {
                Login = user.Login
            };

            Console.WriteLine("Ban user rollback");
            await publishEndpoint.Publish(banUserRollback);
        }
        Console.WriteLine("User not banned");
    }
}