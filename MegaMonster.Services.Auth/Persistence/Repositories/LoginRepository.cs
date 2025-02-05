using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Repositories;

public class LoginRepository(AppDbContext db, PasswordHasher<Users> passwordHasher) : ILoginRepository
{
    public async Task<bool> CheckLoginAndEmail(string login, string email) =>
        await db.Users.AnyAsync(u => u.Email == email && u.Login == login);

    public bool VerifyHashedPassword(Users user, string password) =>
        user.PasswordHash != null && passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;
    
    public async Task<Users?> FindUser(string login) =>
        await db.Users.FirstOrDefaultAsync(u => u.Login == login);
}