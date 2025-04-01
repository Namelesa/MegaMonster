using MassTransit;
using MegaMonster.MessagingModels.UserInformation.UserEmail;
using MegaMonster.Services.User.Application.User;

namespace MegaMonster.Services.User.Application.Messaging;

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