using MegaMonster.Services.Auth.Core.User;
using MegaMonster.Services.Auth.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Repositories;

public class LoginRepository(AppDbContext db) : ILoginRepository
{
    public async Task<bool> CheckLoginAndEmail(string login, string email) =>
        await db.Users.AnyAsync(u => u.Email == email && u.Login == login && u.IsBan == false);
    
    public async Task<Users?> FindUser(string login) =>
        await db.Users.FirstOrDefaultAsync(u => u.Login == login);
    
    public async Task<Users?> FindByEmail(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> UpdateUser(Users user)
    {
        try
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
    }
}