using System.Diagnostics;
using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Repositories;

public class RegisterRepository(AppDbContext db, PasswordHasher<Users> passwordHasher) : IRegisterRepository
{
    public Task<string> HashPassword(string password, Users? user)
    {
        Debug.Assert(user != null, nameof(user) + " != null");
        return Task.FromResult(passwordHasher.HashPassword(user, password));
    }

    public async Task<bool> CheckLoginAndEmail(string login, string email)
    {
        return await db.Users.AnyAsync(u => (u.Email == email && u.Login == login));
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
}