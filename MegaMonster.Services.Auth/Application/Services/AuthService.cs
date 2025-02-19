using MassTransit;
using MegaMonster.Services.Auth.Application.ResultOperation;
using MegaMonster.Services.Auth.Application.Validation;
using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.JWT;
using MessagingModels.UserAdding;
using MessagingModels.UserInformation;

namespace MegaMonster.Services.Auth.Application.Services;

public class AuthService(
    IRegisterRepository registerRepository, 
    ILoginRepository loginRepository, 
    JwtService jwtService, 
    UserValidator userValidator,
    IPublishEndpoint publishEndpoint,
    IRequestClient<UserRequest> userRequestClient)
{
    public async Task<OperationResult> RegisterUser(string password, string email, string userName, string login, string phoneNumber, string? role = null)
    {
        var checkLoginAndEmail = await registerRepository.CheckLoginAndEmail(login, email);
        if (checkLoginAndEmail) return OperationResult.Fail("User with the same email\n" +
                                                            "or login already exists");
        
        var checkUser = await loginRepository.FindUser(login);
        if (checkUser != null && checkUser.IsBan) return OperationResult.Fail("user is baned");
        
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
        
        if (role == null)
        {
            user.Role = Wc.CustomerRole;
            var userMessage = new UserModelMessage(user.Login, user.UserName, user.Email, user.PasswordHash, user.PhoneNumber, user.Role);
            bool result = await registerRepository.RegisterUser(user);
            await publishEndpoint.Publish(userMessage);
            return result ? OperationResult.Ok("Add new Customer") : OperationResult.Fail("Can not register user"); 
        }
        
        user.Role = Wc.AdminRole;
        
        bool resultAddAdmin = await registerRepository.RegisterUser(user);
        return resultAddAdmin ? OperationResult.Ok("Add new Admin") : OperationResult.Fail("Can not register admin user"); 
    }
    
    public async Task<OperationResult> LoginUser(string password, string email, string login)
    {
        var checkLoginAndEmail = await registerRepository.CheckLoginAndEmail(login, email);
        if (!checkLoginAndEmail) return OperationResult.Fail($"User with login '{login}' or with this email '{email}' not found\n" +
                                                             $"or user is baned");
        
        var user = await loginRepository.FindUser(login);
        if (user == null) return OperationResult.Fail("User not found");
        
        var validationResult = await ValidateInfo(user);
        if (!validationResult.Success)
        {
            return validationResult;
        }
        
        user.PasswordHash = await registerRepository.HashPassword(password, user);

        try
        {
            var response = await userRequestClient.GetResponse<UserTicketModel>(new UserRequest(login));

            if (string.IsNullOrEmpty(response.Message.UserId))
            {
                return OperationResult.Fail("User not found in UserService");
            }

            user.Id = response.Message.UserId;
            
            var res = await jwtService.AuthenticateAsync(user, password, user.Role) is { Length: > 0 } token ? token : null;
            return OperationResult.Ok(res);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<OperationResult> BanUser(string email)
    {
        var result = await registerRepository.BanUser(email);
        if (result == "User not found") return OperationResult.Fail(result);

        return OperationResult.Ok(result);
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