using MegaMonster.Services.Auth.Application.ResultOperation;
using MegaMonster.Services.Auth.Application.Validation;
using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.JWT;

namespace MegaMonster.Services.Auth.Application.Services;

public class AuthService(IRegisterRepository registerRepository, ILoginRepository loginRepository, JwtService jwtService, UserValidator userValidator)
{
    public async Task<OperationResult> RegisterUser(string password, string email, string userName, string login, string phoneNumber)
    {
        var checkLoginAndEmail = await registerRepository.CheckLoginAndEmail(login, email);
        if (checkLoginAndEmail) return OperationResult.Fail("User with the same email or login already exists.");
        
        Users user = new Users()
        {
            Email = email,
            UserName = userName,
            Login = login,
            PhoneNumber = phoneNumber,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = userName.ToUpper(),
        };
        user.PasswordHash = await registerRepository.HashPassword(password, user);
        
        var validationResult = await ValidateInfo(user);
        if (!validationResult.Success)
        {
            return validationResult;
        }
        
        bool result = await registerRepository.RegisterUser(user);
        return result ? OperationResult.Ok("") : OperationResult.Fail("Can not register user");
    }
    
    public async Task<OperationResult> LoginUser(string password, string email, string login)
    {
        var checkLoginAndEmail = await registerRepository.CheckLoginAndEmail(login, email);
        if (!checkLoginAndEmail) return OperationResult.Fail($"User with login '{login}' or with this email '{email}' not found");
        
        var user = await loginRepository.FindUser(login);
        if (user == null) return OperationResult.Fail("User not found");
        
        var validationResult = await ValidateInfo(user);
        if (!validationResult.Success)
        {
            return validationResult;
        }
        
        user.PasswordHash = await registerRepository.HashPassword(password, user);
        
        var res = await jwtService.AuthenticateAsync(user, password) is { Length: > 0 } token ? token : null;
        return OperationResult.Ok(res);
    }
    
    private async Task<OperationResult> ValidateInfo(Users user)
    {
        var validationResult = await userValidator.ValidateAsync(user);
        if (validationResult.Errors.Any())
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return OperationResult.Fail(errors);
        }
        return OperationResult.Ok("");
    }
}