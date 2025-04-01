using MassTransit;
using MegaMonster.MessagingModels.RollBacks.User;
using MegaMonster.MessagingModels.UserAdding;
using MegaMonster.Services.User.Application.Role;
using MegaMonster.Services.User.Application.User;
using MegaMonster.Services.User.Core.User;

namespace MegaMonster.Services.User.Application.Messaging;

public class UserConsumer(UserService userService, RoleService roleService, IPublishEndpoint publishEndpoint) : IConsumer<UserModelMessage>
{
    public async Task Consume(ConsumeContext<UserModelMessage> context)
    {
        var user = context.Message;
        Console.WriteLine($"Register with name {user.UserName} was adding");
        
        var currentRole = await roleService.FindRoleByNameAsync(user.Role);
        if (currentRole == null) return;
        
        Users userToAdd = new Users(user.Login)
        {
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            NormalizedEmail = user.NormalizedEmail,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = currentRole,
            RoleId = currentRole.Id,
            PasswordHash = user.Password
        };
        
        var result = await userService.AddUser(userToAdd, currentRole.RoleName);
        if (!result.Success)
        {
            Console.WriteLine("Rollback from User Service");
            var userRollBack = new RegisterUserRollBack(user.Login);
        
            await publishEndpoint.Publish(userRollBack);
        }
        
        await Task.CompletedTask;
    }
}