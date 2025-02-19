using MegaMonster.Services.Auth.Core.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Repositories;

public class LoginRepository(AppDbContext db) : ILoginRepository
{
    public async Task<bool> CheckLoginAndEmail(string login, string email) =>
        await db.Users.AnyAsync(u => u.Email == email && u.Login == login && u.IsBan == false);
    
    public async Task<Users?> FindUser(string login) =>
        await db.Users.FirstOrDefaultAsync(u => u.Login == login);
}