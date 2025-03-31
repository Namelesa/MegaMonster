using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Core.Interfaces;

public interface IBannedUserRepository
{
    Task<bool> AddAsync(Users t);
    Task<bool> DeleteAsync(Users t);
    Task<Users?> FindByLoginAsync(string login);
}