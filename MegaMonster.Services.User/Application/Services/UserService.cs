using MegaMonster.Services.User.Application.ResultOperation;
using MegaMonster.Services.User.Application.Validation.UserValidator;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Services;

public class UserService(IUserRepository userRepository, UserValidation validation)
{
    public async Task<IEnumerable<Users>> GetAllUsers()
    {
        return await userRepository.GetAllAsync();
    }

    public async Task<OperationResult> AddUser(Users user)
    {
        var validationResult = await ValidateUser(user);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        bool result = await userRepository.AddAsync(user);
        return result ? OperationResult.Ok() : OperationResult.Fail("Failed to add user.");
    }

    public async Task<OperationResult> EditUser(string login, string userName, string email, string phoneNumber, string newLogin)
    {
        var user = await userRepository.GetUserByLoginAsync(login);
        if (user == null) return OperationResult.Fail($"User with login '{login}' not found.");

        UpdateUserInfo(user, userName, email, phoneNumber, newLogin);
        
        var validationResult = await ValidateUser(user);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        bool result = await userRepository.EditAsync(user);
        return result ? OperationResult.Ok() : OperationResult.Fail("Failed to edit user.");
    }

    public async Task<OperationResult> DeleteUser(string login)
    {
        var user = await userRepository.GetUserByLoginAsync(login);
        if (user == null) return OperationResult.Fail($"User with login '{login}' not found.");

        bool result = await userRepository.DeleteAsync(user);
        return result ? OperationResult.Ok() : OperationResult.Fail("Failed to delete user.");
    }

    private async Task<OperationResult> ValidateUser(Users user)
    {
        var validationResult = await validation.ValidateAsync(user);
        if (validationResult.Errors.Any())
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return OperationResult.Fail(errors);
        }
        return OperationResult.Ok();
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
}
