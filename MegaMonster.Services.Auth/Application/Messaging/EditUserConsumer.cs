using MassTransit;
using MegaMonster.MessagingModels.User.Edit;
using MegaMonster.MessagingModels.User.RollBacks;
using MegaMonster.Services.Auth.Application.User;

namespace MegaMonster.Services.Auth.Application.Messaging;

public class EditUserConsumer(AuthService authService, IPublishEndpoint publishEndpoint) : IConsumer<UserEditMessage>
{
    public async Task Consume(ConsumeContext<UserEditMessage> context)
    {
        var userEdit = context.Message;
        
        var result = await authService.EditUser(userEdit.OldLogin, userEdit.NewUserName, userEdit.NewEmail, userEdit.NewPhoneNumber, userEdit.NewLogin);

        if (!result.Success)
        {
            var rollback = new EditUserInfoRollBack
            {
                Email = userEdit.Email,
                Login = userEdit.OldLogin,
                PhoneNumber = userEdit.PhoneNumber,
                UserName = userEdit.UserName,
                NewLogin = userEdit.NewLogin
            };
            
            Console.WriteLine($"Old login = {rollback.Login}");
            Console.WriteLine($"Email = {rollback.Email}");
            Console.WriteLine($"Phone number = {rollback.PhoneNumber}");
            Console.WriteLine($"New Login = {rollback.NewLogin}");
            
            await publishEndpoint.Publish(rollback);
            Console.WriteLine("Rollback for edit user info");
        }
    }
}