using MegaMonster.Services.Auth.Core.Models;

namespace MegaMonster.Services.Auth.Core.Interfaces;

public interface IRegisterRepository : IRepository<Users>
{
    Task<bool> RegisterUser(Users? user);
    Task<string> HashPassword(string password, Users? user);
}