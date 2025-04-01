using MegaMonster.Services.Auth.Core.Interfaces;

namespace MegaMonster.Services.Auth.Core.User;

public interface ILoginRepository : IRepository<Users>
{
    Task<Users?> FindUser(string login);
    Task<Users?> FindByEmail(string email);
    Task<bool> UpdateUser(Users user);
}