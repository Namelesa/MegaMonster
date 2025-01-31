using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Core.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    public Task<Role?> FindRoleByNameAsync(string name);
    public Task<List<Role>> GetRolesByNamesAsync(string oldName, string newName);
    
}