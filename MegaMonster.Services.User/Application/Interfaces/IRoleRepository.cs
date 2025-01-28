using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    public Task<Role?> FindRoleByNameAsync(string name);
}