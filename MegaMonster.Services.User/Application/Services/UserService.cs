using MassTransit;
using MegaMonster.Services.User.Application.ResultOperation;
using MegaMonster.Services.User.Application.Validation.UserValidator;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.Infrastructure.Redis;
using MessagingModels.UserNotification;

namespace MegaMonster.Services.User.Application.Services;

public class UserService(IUserRepository userRepository, 
    UserValidation validation, 
    IRedisService redisService,
    IPublishEndpoint publishEndpoint)
{
    private const string UsersCacheKey = "All_Users";

    public async Task<IEnumerable<Users>> GetAllUsers() =>
        await GetOrSetCache(UsersCacheKey, userRepository.GetAllAsync!);

    public async Task<Users?> FindByLoginAsync(string login)
    {
        return await userRepository.GetUserByLoginAsync(login);
    }

    public async Task<OperationResult> AddUser(Users user) =>
        await HandleDatabaseOperation(async () =>
        {
            var validationResult = await ValidateUser(user);
            if (!validationResult.Success) return validationResult;

            bool result = await userRepository.AddAsync(user);
            if (!result) return OperationResult.Fail("Failed to add user.");
            
            var notify = new UserNotificationBase(user.UserName, user.Email);
            Console.WriteLine($"Publishing notification for {user.UserName}, ID: {Guid.NewGuid()}");
            await publishEndpoint.Publish(notify);
            
            return OperationResult.Ok();
        });

    public async Task<OperationResult> EditUser(string login, string userName, string email, string phoneNumber, string newLogin) =>
        await HandleDatabaseOperation(async () =>
        {
            var user = await userRepository.GetUserByLoginAsync(login);
            if (user is null) return OperationResult.Fail($"User with login '{login}' not found.");

            UpdateUserInfo(user, userName, email, phoneNumber, newLogin);

            var validationResult = await ValidateUser(user);
            if (!validationResult.Success) return validationResult;

            return await userRepository.EditAsync(user)
                ? OperationResult.Ok()
                : OperationResult.Fail("Failed to edit user.");
        });

    public async Task<OperationResult> DeleteUser(string login, string reason) =>
        await HandleDatabaseOperation(async () =>
        {
            var user = await userRepository.GetUserByLoginAsync(login);
            if (user is null) return OperationResult.Fail($"User with login '{login}' not found.");

            bool result = await userRepository.DeleteAsync(user);
            if (!result) return OperationResult.Fail("Failed to delete user.");
            
            var notify = new UserBan(user.Login, user.Email, reason);
            
            await publishEndpoint.Publish(notify);
            
            return OperationResult.Ok();
        });

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
