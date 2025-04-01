using System.Diagnostics;
using MegaMonster.Services.Auth.Core.User;
using MegaMonster.Services.Auth.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Repositories;

public class RegisterRepository(AppDbContext db, PasswordHasher<Users> passwordHasher) : IRegisterRepository
{
    public async Task<Users?> FindUserByEmail(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<string> HashPassword(string password, Users? user)
    {
        Debug.Assert(user != null, nameof(user) + " != null");
        return Task.FromResult(passwordHasher.HashPassword(user, password));
    }

    public async Task<bool> ConfirmEmailAsync(Users user)
    {
        try
        {
            user.EmailConfirmed = true;
            await db.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    
    public async Task<string> BanUser(string email)
    {
        var user = await db.Users.FirstOrDefaultAsync(u=> u.Email == email && !u.IsBan);
        if (user == null) return "Register not found";

        user.IsBan = true;
        await db.SaveChangesAsync();
        return "Baned user";
    }
    
    public async Task<bool> CheckLoginAndEmail(string login, string email)
    {
        return await db.Users.AnyAsync(u => (u.Email == email && u.Login == login && u.IsBan == false));
    }

    public async Task<bool> RegisterUser(Users? user)
    {
        try
        {
            if (user != null) db.Users.Add(user);
            await db.SaveChangesAsync();
            return true;
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteUserByLogin(string login)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user == null) return false;

        try
        {
            db.Users.Remove(user);
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