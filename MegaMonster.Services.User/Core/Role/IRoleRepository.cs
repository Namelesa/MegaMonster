using MegaMonster.Services.User.Core.Interfaces;

namespace MegaMonster.Services.User.Core.Role;

public interface IRoleRepository : IRepository<Role>
{
    public Task<Role?> FindRoleByNameAsync(string name);
    public Task<List<Role>> GetRolesByNamesAsync(string oldName, string newName);
    
}