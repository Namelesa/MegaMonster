using MegaMonster.Services.Auth.Core.Models;

namespace MegaMonster.Services.Auth.Core.Interfaces;

public interface ILoginRepository : IRepository<Users>
{
    Task<Users?> FindUser(string login);
    Task<Users?> FindByEmail(string email);
    Task<bool> UpdateUser(Users user);
}