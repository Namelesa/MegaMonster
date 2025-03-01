using MassTransit;
using MegaMonster.MessagingModels.EditUserMessage;
using MegaMonster.Services.Auth.Application.Services;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class EditUserConsumer(AuthService authService) : IConsumer<UserEditMessage>
{
    public async Task Consume(ConsumeContext<UserEditMessage> context)
    {
        var userEdit = context.Message;
        
        await authService.EditUser(userEdit.OldLogin, userEdit.UserName, userEdit.Email, userEdit.PhoneNumber, userEdit.NewLogin);
    }
}