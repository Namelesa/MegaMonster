using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MegaMonster.Services.Auth.Core.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MegaMonster.Services.Auth.Infrastructure.JWT;

public class JwtService(IConfiguration config, ILogger<JwtService> logger)
{
    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ILogger<JwtService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PasswordHasher<Users> _passwordHasher = new();

    public async Task<string?> AuthenticateAsync(Users? user, string passwordRequest, string role)
    {
        if (!ValidateUserCredentials(user, passwordRequest))
        {
            _logger.LogWarning("Authentication failed: invalid credentials for user {UserLogin}", user?.Login);
            return null;
        }

        try
        {
            var token = GenerateJwtToken(user, role);
            return await Task.FromResult(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating JWT token for user {UserLogin}", user?.Login);
            return null;
        }
    }

    private bool ValidateUserCredentials(Users? user, string passwordRequest)
    {
        if (user?.PasswordHash == null) return false;

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, passwordRequest);
        return passwordVerificationResult == PasswordVerificationResult.Success;
    }

    private string GenerateJwtToken(Users? user, string role)
    {
        var issuer = _config["JWTConfig:Issuer"];
        var audience = _config["JWTConfig:Audience"];
        var key = _config["JWTConfig:Key"];
        var tokenMin = _config.GetValue<int>("JWTConfig:TokenValidityMinutes");

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT key is not configured properly.");
        }

        var tokenExpiry = DateTime.UtcNow.AddMinutes(tokenMin);
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        Debug.Assert(user != null, nameof(user) + " != null");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = tokenExpiry,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}
