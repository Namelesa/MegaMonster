using MassTransit;
using MegaMonster.MessagingModels.UserInformation;
using MegaMonster.Services.User.Application.User;

namespace MegaMonster.Services.User.Application.Messaging;

public class UserConfirmConsumer(UserService userService) : IConsumer<ConfirmEmailUser>
{
    public async Task Consume(ConsumeContext<ConfirmEmailUser> context)
    {
        var message = context.Message;
        
        await userService.ConfirmEmail(message.Login);
    }
}