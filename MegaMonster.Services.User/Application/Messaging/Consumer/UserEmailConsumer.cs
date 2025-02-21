using MassTransit;
using MegaMonster.Services.User.Application.Services;
using MegaMonster.MessagingModels.UserInformation.UserEmail;

namespace MegaMonster.Services.User.Application.Messaging.Consumer;

public class UserEmailConsumer(UserService userService) : IConsumer<UserEmailRequest>
{
    public async Task Consume(ConsumeContext<UserEmailRequest> context)
    {
        var user = await userService.FindByIdAsync(context.Message.Id);
        if (user == null)
        {
            await context.RespondAsync(new UserEmailResponse{Email = ""});
            Console.WriteLine("Not found user");
        }

        await context.RespondAsync(new UserEmailResponse{Email = user.Email});
    }
}