using MassTransit;
using MegaMonster.MessagingModels.RollBacks.User;
using MegaMonster.Services.User.Application.Services;

namespace MegaMonster.Services.User.Application.Messaging.User;

public class UserEditInfoRollBackConsumer(UserService userService) : IConsumer<EditUserInfoRollBack>
{
    public async Task Consume(ConsumeContext<EditUserInfoRollBack> context)
    {
        var userInfo = context.Message;

        Console.WriteLine($"Old login = {userInfo.Login}");
        Console.WriteLine($"Email = {userInfo.Email}");
        Console.WriteLine($"Phone number = {userInfo.PhoneNumber}");
        Console.WriteLine($"New Login = {userInfo.NewLogin}");
        
        var result = await userService.EditUserRollBack(userInfo.Login, userInfo.UserName, userInfo.Email, userInfo.PhoneNumber, userInfo.NewLogin);
        
        if(result.Success) Console.WriteLine(result.Message);
        
        Console.WriteLine("Rollback ok");
        
    }
}