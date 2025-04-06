using MegaMonster.Services.Auth.Core.User;

namespace MegaMonster.Services.Auth.Infrastructure.JWT;

public interface IJwtService
{
    Task<string?> AuthenticateAsync(Users? user, string passwordRequest, string role);
}