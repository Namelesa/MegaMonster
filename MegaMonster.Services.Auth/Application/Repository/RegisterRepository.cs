using MegaMonster.Services.Auth.Application.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Application.Repository;

public class RegisterRepository(AppDbContext db, PasswordHasher<Users?> passwordHasher) : IRegisterRepository
{
    public Task<string> HashPassword(string password, Users? user)
    {
       return Task.FromResult(passwordHasher.HashPassword(user, password));
    }

    public async Task<bool> CheckLoginAndEmail(string login, string email)
    {
        return !await db.Users.AnyAsync(u => u.Email == email || u.Login == login);
    }

    public async Task<bool> RegisterUser(Users? user)
    {
        try
        {
            db.Users.Add(user);
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