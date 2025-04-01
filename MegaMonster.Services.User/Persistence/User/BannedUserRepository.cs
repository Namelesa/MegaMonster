using MegaMonster.Services.User.Core.User;
using MegaMonster.Services.User.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Persistence.User;

public class BannedUserRepository(AppDbContext db) : IBannedUserRepository
{
    public async Task<bool> AddAsync(Users t)
    {
        try
        {
            await db.BannedUsers.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Users t)
    {
        try
        {
            db.BannedUsers.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<Users?> FindByLoginAsync(string login)
    {
        return await db.BannedUsers.FirstOrDefaultAsync(u => u.Login == login);
    }
}