namespace MegaMonster.Services.User.Core.User;

public interface IBannedUserRepository
{
    Task<bool> AddAsync(Users t);
    Task<bool> DeleteAsync(Users t);
    Task<Users?> FindByLoginAsync(string login);
}