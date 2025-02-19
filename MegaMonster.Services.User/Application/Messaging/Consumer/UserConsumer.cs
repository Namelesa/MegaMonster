using MassTransit;
using MegaMonster.Services.User.Application.Services;
using MegaMonster.Services.User.Core.Models;
using MessagingModels.UserAdding;

namespace MegaMonster.Services.User.Application.Messaging.Consumer;

public class UserConsumer(UserService userService, RoleService roleService) : IConsumer<UserModelMessage>
{
    public async Task Consume(ConsumeContext<UserModelMessage> context)
    {
        var user = context.Message;
        Console.WriteLine($"User with name {user.UserName} was adding");
        
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
        
        await userService.AddUser(userToAdd, currentRole.RoleName);
        
        await Task.CompletedTask;
    }
}