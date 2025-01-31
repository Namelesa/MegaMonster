using MegaMonster.Services.Auth.Application.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.Data;
using MegaMonster.Services.Auth.Infrastructure.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Application.Repository;

public class LoginRepository(AppDbContext db, JwtService jwtService, PasswordHasher<Users> passwordHasher) : ILoginRepository
{
    public async Task<bool> CheckLoginAndEmail(string login, string email) =>
        !await db.Users.AnyAsync(u => u.Email == email || u.Login == login);

    public bool VerifyHashedPassword(Users user, string password) =>
        user.PasswordHash != null && passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;

    public async Task<string?> Auth(Users? user, string password) =>
        await jwtService.Authenticate(user, password) is { Length: > 0 } token ? token : null;

    public async Task<Users?> FindUser(string login) =>
        await db.Users.FirstOrDefaultAsync(u => u.Login == login);
}