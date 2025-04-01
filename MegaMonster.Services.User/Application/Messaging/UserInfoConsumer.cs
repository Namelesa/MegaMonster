using MassTransit;
using MegaMonster.MessagingModels.User.GetInfo;
using MegaMonster.MessagingModels.User.Notification;
using MegaMonster.MessagingModels.User.Tickets;
using MegaMonster.Services.User.Application.User;

namespace MegaMonster.Services.User.Application.Messaging;

public class UserInfoConsumer(UserService userService, ILogger<UserInfoConsumer> logger) : IConsumer<UserRequest>
{
    public async Task Consume(ConsumeContext<UserRequest> context)
    {
        var login = context.Message.Login;

        var user = await userService.FindByLoginAsync(login);
        if (user == null)
        {
            logger.LogWarning($"Register with login =  {login} not found");
            await context.RespondAsync(new UserTicketModel("", ""));
            return;
        }

        logger.LogInformation($"Send ID {user.Id} and name {user.UserName}");

        if (user.UserName != null) await context.RespondAsync(new UserTicketModel(user.Id, user.UserName));
    }
}