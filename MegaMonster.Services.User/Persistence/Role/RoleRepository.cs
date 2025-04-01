using MegaMonster.Services.User.Core.Role;
using MegaMonster.Services.User.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Persistence.Role;

public class RoleRepository(AppDbContext db, ILogger<RoleRepository> logger) : IRoleRepository
{
    public async Task<IEnumerable<Core.Role.Role>> GetAllAsync()
    {
        return await db.Roles.ToListAsync<Core.Role.Role>();
    }

    public async Task<bool> AddAsync(Core.Role.Role role)
    {
        try
        {
            await db.Roles.AddAsync(role);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding role: {Role}", role);
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.Role.Role role)
    {
        try
        {
            db.Roles.Update(role);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating role: {Role}", role);
            return false;
        }
    }
    
    public async Task<bool> DeleteAsync(Core.Role.Role role)
    {
        try
        {
            db.Roles.Remove(role);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user: {Role}", role);
            return false;
        }
    }
    
    public async Task<Core.Role.Role?> FindRoleByNameAsync(string name)
    {
         var role = await db.Roles.FirstOrDefaultAsync<Core.Role.Role>(u => u.RoleName == name);
         return role;
    }
    public async Task<List<Core.Role.Role>> GetRolesByNamesAsync(string oldName, string newName)
    {
        return await Queryable
            .Where<Core.Role.Role>(db.Roles, r => r.RoleName == oldName || r.RoleName == newName)
            .AsNoTracking()
            .ToListAsync();
    }

}