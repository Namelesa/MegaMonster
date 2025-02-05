using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Persistence.Repositories;

public class RoleRepository(AppDbContext db, ILogger<RoleRepository> logger) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await db.Roles.ToListAsync();
    }

    public async Task<bool> AddAsync(Role role)
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

    public async Task<bool> EditAsync(Role role)
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
    
    public async Task<bool> DeleteAsync(Role role)
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
    
    public async Task<Role?> FindRoleByNameAsync(string name)
    {
         var role = await db.Roles.FirstOrDefaultAsync(u => u.RoleName == name);
         return role;
    }
    public async Task<List<Role>> GetRolesByNamesAsync(string oldName, string newName)
    {
        return await db.Roles
            .Where(r => r.RoleName == oldName || r.RoleName == newName)
            .AsNoTracking()
            .ToListAsync();
    }

}