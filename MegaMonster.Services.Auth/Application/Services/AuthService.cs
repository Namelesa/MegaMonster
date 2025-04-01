using MassTransit;
using MegaMonster.Services.Auth.Application.ResultOperation;
using MegaMonster.Services.Auth.Application.Validation;
using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.JWT;
using MegaMonster.MessagingModels.UserAdding;
using MegaMonster.MessagingModels.UserInformation;

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
        if (checkLoginAndEmail) return OperationResult.Fail("Register with the same email\n" +
                                                            "or login already exists");
        
        var checkUser = await loginRepository.FindUser(login);
        if (checkUser is { IsBan: true }) return OperationResult.Fail("user is baned");
        
        Users user = new Users
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
        if (!validationResult.Success) return validationResult;
        
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
        if (!checkLoginAndEmail) return OperationResult.Fail($"Register with login '{login}' or with this email '{email}' not found\n" +
                                                             $"or user is baned");
        
        var user = await loginRepository.FindUser(login);
        if (user == null) return OperationResult.Fail("Register not found");
        
        if(!user.EmailConfirmed) return OperationResult.Fail("Please confirm email");
        
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
                return OperationResult.Fail("Register not found in UserService");
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
        if (result == "Register not found")
        {
            return OperationResult.Fail(result);
        }
        return OperationResult.Ok(result);
    }
    
    public async Task<OperationResult> DeleteUser(string login)
    {
        var result = await registerRepository.DeleteUserByLogin(login);
        return result ? OperationResult.Ok($"Register with login: {login} was deleted") : OperationResult.Fail($"Can not delete user with login {login}");
    }
    
    public async Task<OperationResult> ConfirmEmail(string email)
    {
        var user = await registerRepository.FindUserByEmail(email);
        if (user == null) return OperationResult.Fail("Register not found");

        var result = await registerRepository.ConfirmEmailAsync(user);
        
        if (result)
        {
            await publishEndpoint.Publish(new ConfirmEmailUser(user.Login));
            return OperationResult.Ok("Email confirmed successfully");
        }
        return OperationResult.Fail("Invalid or expired token");
    }
    
    public async Task<OperationResult> EditUser(string oldLogin, string userName, string email, string phoneNumber, string newLogin) 
    {
        var user = await loginRepository.FindUser(oldLogin);
        if (user == null) return OperationResult.Fail("Register not found");

        user.UserName = userName;
        user.Login = newLogin;
        user.PhoneNumber = phoneNumber;
        user.Email = email;
        user.NormalizedUserName = userName.ToUpper();
        user.NormalizedEmail = email.ToUpper();
        
        var result = await loginRepository.UpdateUser(user);
        return result ? OperationResult.Ok("Register edited successfully") : OperationResult.Fail("Error with editing user info");
    }
    
    public async Task<Users?> FindUserByEmail(string email)
    {
        return await loginRepository.FindByEmail(email);
    }
    
    public async Task<Users?> FindUserByLogin(string login)
    {
        return await loginRepository.FindUser(login);
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