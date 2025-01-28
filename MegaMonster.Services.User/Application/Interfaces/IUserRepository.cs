using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Interfaces;

public interface IUserRepository : IRepository<Users>
{
    public Task<Users?> GetUserByLoginAsync(string login);
}