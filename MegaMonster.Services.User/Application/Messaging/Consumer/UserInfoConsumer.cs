using MassTransit;
using MegaMonster.Services.User.Application.Services;
using MessagingModels.UserInformation;

namespace MegaMonster.Services.User.Application.Messaging.Consumer;

public class UserInfoConsumer(UserService userService, ILogger<UserInfoConsumer> logger) : IConsumer<UserRequest>
{
    public async Task Consume(ConsumeContext<UserRequest> context)
    {
        var login = context.Message.Login;

        var user = await userService.FindByLoginAsync(login);
        if (user == null)
        {
            logger.LogWarning($"User with login =  {login} not found");
            await context.RespondAsync(new UserTicketModel("", ""));
            return;
        }

        logger.LogInformation($"Send ID {user.Id} and name {user.UserName}");

        if (user.UserName != null) await context.RespondAsync(new UserTicketModel(user.Id, user.UserName));
    }
}