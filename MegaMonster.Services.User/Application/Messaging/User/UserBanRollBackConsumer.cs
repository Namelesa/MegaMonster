using MassTransit;
using MegaMonster.MessagingModels.RollBacks.User;
using MegaMonster.Services.User.Application.Services;

namespace MegaMonster.Services.User.Application.Messaging.User;

public class UserBanRollBackConsumer(UserService userService) : IConsumer<BanUserRollBack>
{
    public async Task Consume(ConsumeContext<BanUserRollBack> context)
    {
        var login = context.Message.Login;

        await userService.UserBanRestoreAsync(login);
    }
}