using MassTransit;
using MegaMonster.MessagingModels.RollBacks.User;
using MegaMonster.Services.Auth.Application.User;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class RollBackConsumer(AuthService authService) : IConsumer<RegisterUserRollBack>
{
    public async Task Consume(ConsumeContext<RegisterUserRollBack> context)
    {
        var user = context.Message;
        Console.WriteLine($"Start rollback for user with login {user.Login}");

        await authService.DeleteUser(user.Login);
    }
}