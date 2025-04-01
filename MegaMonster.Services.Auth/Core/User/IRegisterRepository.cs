using MegaMonster.Services.Auth.Core.Interfaces;

namespace MegaMonster.Services.Auth.Core.User;

public interface IRegisterRepository : IRepository<Users>
{
    Task<bool> RegisterUser(Users? user);
    Task<bool> ConfirmEmailAsync(Users user);
    Task<Users?> FindUserByEmail(string email);
    Task<string> HashPassword(string password, Users? user);
    Task<string> BanUser(string email);
    Task<bool> DeleteUserByLogin(string login);
}