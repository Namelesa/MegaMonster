using MegaMonster.Services.Auth.Core.Models;

namespace MegaMonster.Services.Auth.Application.Interfaces;

public interface ILoginRepository : IRepository<Users>
{
    bool VerifyHashedPassword (Users user, string password);
    Task<string?> Auth (Users? user, string password);

    Task<Users?> FindUser(string login);
}