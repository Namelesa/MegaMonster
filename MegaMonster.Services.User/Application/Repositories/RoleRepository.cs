using MegaMonster.Services.User.Application.Interfaces;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Application.Repositories;

public class RoleRepository(AppDbContext db) : IRoleRepository
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
        catch
        {
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
        catch
        {
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
        catch
        {
            return false;
        }
    }
    
    public async Task<Role?> FindRoleByNameAsync(string name)
    {
        return await db.Roles.FirstOrDefaultAsync(u => u.RoleName == name);
    }
}