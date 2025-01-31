using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.Infrastructure.Data;
using MegaMonster.Services.Auth.WebApi.Dto_s;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MegaMonster.Services.Auth.Infrastructure.JWT;

public class JwtService(AppDbContext db, IConfiguration config)
{
    private readonly PasswordHasher<Users?> _passwordHasher = new();
    //private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<string?> Authenticate(Users? user, string passwordRequest)
    {
        if (user.PasswordHash != null)
        {
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, passwordRequest);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
                return "Null";
        }

        var issuer = config["JWTConfig:Issuer"];
        var audience = config["JWTConfig:Audience"];
        var key = config["JWTConfig:Key"];
        var tokenMin = config.GetValue<int>("JWTConfig:TokenValidityMinutes");
        var tokenExpiryTimeStep = DateTime.UtcNow.AddMinutes(tokenMin);

        Debug.Assert(key != null, nameof(key) + " != null");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Name, user.Login)
            }),
            Expires = tokenExpiryTimeStep,
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