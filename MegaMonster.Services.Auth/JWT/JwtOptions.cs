namespace MegaMonster.Services.Auth.JWT;

public class JwtOptions
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Key { get; set; }
    public string? TokenValidityMinutes { get; set; }
}