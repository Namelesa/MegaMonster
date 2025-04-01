using MassTransit;
using MegaMonster.MessagingModels.User;
using MegaMonster.MessagingModels.User.AddAdmin;
using MegaMonster.MessagingModels.User.Edit;
using MegaMonster.MessagingModels.User.Notification;
using MegaMonster.MessagingModels.User.Notification.Ban;
using MegaMonster.Services.User.Core.User;
using MegaMonster.Services.User.Infrastructure.Redis;

namespace MegaMonster.Services.User.Application.User;

public class UserService(IUserRepository userRepository, 
    UserValidation validation, 
    IRedisService redisService,
    IPublishEndpoint publishEndpoint,
    IBannedUserRepository bannedUserRepository)
{
    private const string UsersCacheKey = "All_Users";

    public async Task<IEnumerable<Users>> GetAllUsers() =>
        await GetOrSetCache(UsersCacheKey, userRepository.GetAllAsync!);
    public async Task<Users?> FindByLoginAsync(string login)
    {
        return await userRepository.GetUserByLoginAsync(login);
    }
    public async Task<Users?> FindByIdAsync(Guid userId)
    {
        return await userRepository.GetUserByIdAsync(userId);
    }
    public async Task<OperationResult> AddUser(Users user, string role) =>
        await HandleDatabaseOperation(async () =>
        {
            var validationResult = await ValidateUser(user);
            if (!validationResult.Success) return validationResult;
            
            bool result = await userRepository.AddAsync(user);
            if (!result) return OperationResult.Fail("Failed to add user.");

            if (role == Wc.CustomerRole)
            {
                var confirmationLink = $"https://localhost/auth/confirm-email?email={user.Email}";
                var notify = new UserNotificationBase(user.UserName, user.Email, confirmationLink);
                Console.WriteLine($"Publishing notification for {user.UserName}, ID: {Guid.NewGuid()}");
                await publishEndpoint.Publish(notify);
            }
            else
            {
                var addAdmin = new AddAdminModel
                {
                    Login = user.Login,
                    UserName = user.UserName,
                    Email = user.Email,
                    Password = user.PasswordHash,
                    PhoneNumber = user.PhoneNumber,
                    Role = role
                };
                Console.WriteLine($"Publishing message fot adding Admin for {user.UserName}, ID: {Guid.NewGuid()}");
                await publishEndpoint.Publish(addAdmin);
            }
            
            return OperationResult.Ok();
        });
    public async Task<OperationResult> EditUser(string login, string userName, string email, string phoneNumber, string newLogin) =>
        await HandleDatabaseOperation(async () =>
        {
            var user = await userRepository.GetUserByLoginAsync(login);
            var currentUser = user;
            if (user is null) return OperationResult.Fail($"Register with login '{login}' not found.");
            
            UpdateUserInfo(user, userName, email, phoneNumber, newLogin);

            var validationResult = await ValidateUser(user);
            if (!validationResult.Success) return validationResult;
            
            var result = await userRepository.EditAsync(user);
            if (!result) return OperationResult.Fail("Can not update Register");
            
            var updateUserMessage = new UserEditMessage(newLogin, login, currentUser.Email, currentUser.UserName, currentUser.PhoneNumber, email, userName, phoneNumber);
            await publishEndpoint.Publish(updateUserMessage);

            return OperationResult.Ok();
        });
    public async Task<OperationResult> EditUserRollBack(string login, string userName, string email, string phoneNumber, string newLogin) =>
        await HandleDatabaseOperation(async () =>
        {
            var user = await userRepository.GetUserByLoginAsync(newLogin);
            if (user is null) return OperationResult.Fail($"Register with login '{newLogin}' not found.");

            UpdateUserInfo(user, userName, email, phoneNumber, login);

            var validationResult = await ValidateUser(user);
            if (!validationResult.Success) return validationResult;

            var result = await userRepository.EditAsync(user);
            return !result ? OperationResult.Fail("Can not update Register") : OperationResult.Ok();
        });
    public async Task<OperationResult> DeleteUser(string login, string reason) =>
        await HandleDatabaseOperation(async () =>
        {
            var user = await userRepository.GetUserByLoginAsync(login);
            if (user is null) return OperationResult.Fail($"Register with login '{login}' not found.");

            bool result = await userRepository.DeleteAsync(user);
            if (!result) return OperationResult.Fail("Failed to delete user.");

            var banResult = await bannedUserRepository.AddAsync(user);
            if (!banResult) return OperationResult.Fail("Failed to delete user.");
            
            var notify = new UserBan(user.Login, user.Email, reason);
            var banAuth = new UserBanForAuth(user.Email);
            
            await publishEndpoint.Publish(notify);
            await publishEndpoint.Publish(banAuth);
            
            return OperationResult.Ok();
        });
    public async Task<OperationResult> ConfirmEmail(string login)
    {
        var user = await userRepository.GetUserByLoginAsync(login);
        if (user == null)
        {
            return OperationResult.Fail("Register not found");
        }
        var result = await userRepository.ConfirmEmailAsync(user);
        return result ? OperationResult.Ok() : OperationResult.Fail("Can not confirm email");
    }
    public async Task<OperationResult> UserBanRestoreAsync(string login)
    {
        var user = await bannedUserRepository.FindByLoginAsync(login);
        if (user == null)
        {
            return OperationResult.Fail("Register not found");
        }
        var result = await userRepository.AddAsync(user);
        return result ? OperationResult.Ok() : OperationResult.Fail("Can not restore user");
    }
    private async Task<OperationResult> ValidateUser(Users user)
    {
        var validationResult = await validation.ValidateAsync(user);
        return validationResult.Errors.Any()
            ? OperationResult.Fail(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)))
            : OperationResult.Ok();
    }
    private void UpdateUserInfo(Users user, string userName, string email, string phoneNumber, string newLogin)
    {
        user.Login = newLogin;
        user.UserName = userName;
        user.NormalizedUserName = userName.ToUpper();
        user.NormalizedEmail = email.ToUpper();
        user.Email = email;
        user.PhoneNumber = phoneNumber;
    }
    private async Task<T?> GetOrSetCache<T>(string key, Func<Task<T?>> getData, TimeSpan? expiration = null)
    {
        return await redisService.GetAsync<T>(key) 
               ?? await getData().ContinueWith(async task =>
               {
                   var data = await task;
                   if (data is not null)
                       await redisService.SetAsync(key, data, expiration ?? TimeSpan.FromMinutes(60));
                   return data;
               }).Unwrap();
    }
    private async Task<OperationResult> HandleDatabaseOperation(Func<Task<OperationResult>> operation)
    {
        var result = await operation();
        if (result.Success) await redisService.RemoveAsync(UsersCacheKey);
        return result;
    }
}
