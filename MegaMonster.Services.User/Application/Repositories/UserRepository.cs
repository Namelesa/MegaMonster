using MegaMonster.Services.User.Application.Interfaces;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Application.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<IEnumerable<Users>> GetAllAsync()
    {
        return await db.Users.ToListAsync();
    }
    public async Task<bool> AddAsync(Users user)
    {
        try
        {
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EditAsync(Users user)
    {
        try
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Users user)
    {
        try
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<Users?> GetUserByLoginAsync(string login)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Login == login);
    }
}