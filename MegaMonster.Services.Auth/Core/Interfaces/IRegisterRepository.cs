using MegaMonster.Services.Auth.Core.Models;

namespace MegaMonster.Services.Auth.Core.Interfaces;

public interface IRegisterRepository : IRepository<Users>
{
    Task<bool> RegisterUser(Users? user);
    Task<bool> ConfirmEmailAsync(Users user);
    Task<Users?> FindUserByEmail(string email);
    Task<string> HashPassword(string password, Users? user);
    Task<string> BanUser(string email);
}