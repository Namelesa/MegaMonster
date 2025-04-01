using MassTransit;
using MegaMonster.MessagingModels.UserAdding;
using MegaMonster.Services.Auth.Application.User;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class AddAdminConsumer(AuthService authService) : IConsumer<AddAdminModel>
{
    public async Task Consume(ConsumeContext<AddAdminModel> context)
    {
        Console.WriteLine($"Message {context.Message.Role}");

        var message = context.Message;

        var result = await authService.RegisterUser(message.Password, message.Email, message.UserName, message.Login, message.PhoneNumber, message.Role);

        Console.WriteLine($"Operation Result = {result.Success}");
    }
}   