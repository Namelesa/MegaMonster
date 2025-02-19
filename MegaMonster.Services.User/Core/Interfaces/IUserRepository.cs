using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Core.Interfaces;

public interface IUserRepository : IRepository<Users>
{
    public Task<Users?> GetUserByLoginAsync(string login);
    public Task<Users?> GetUserByIdAsync(Guid userId);
}