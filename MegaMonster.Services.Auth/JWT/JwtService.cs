using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MegaMonster.Services.Auth.Data;
using MegaMonster.Services.Auth.Dto_s;
using MegaMonster.Services.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MegaMonster.Services.Auth.JWT;

public class JwtService(AppDbContext db, IOptions<JwtOptions> jwtOptions, IConfiguration config)
{
    private readonly PasswordHasher<Users> _passwordHasher = new();
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<string?> Authenticate(LoginDto request)
    {
        var userAccount = await db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (userAccount == null || string.IsNullOrWhiteSpace(userAccount.PasswordHash))
            return "Null";
        
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(userAccount, userAccount.PasswordHash, request.Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
            return "Null";
        
        var issuer = config["JWTConfig:Issuer"];
        var audience = config["JWTConfig:Audience"];
        var key = config["JWTConfig:Key"];
        var tokenMin = config.GetValue<int>("JWTConfig:TokenValidityMinutes");
        var tokenExpiryTimeStap = DateTime.UtcNow.AddMinutes(tokenMin);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, request.Login)
            }),
            Expires = tokenExpiryTimeStap,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        return $"{accessToken}";
    }
}