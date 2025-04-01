using MegaMonster.Services.User.Core.Interfaces;

namespace MegaMonster.Services.User.Core.User;

public interface IUserRepository : IRepository<Users>
{
    public Task<Users?> GetUserByLoginAsync(string login);
    public Task<Users?> GetUserByIdAsync(Guid userId);

    public Task<bool> ConfirmEmailAsync(Users user);
}