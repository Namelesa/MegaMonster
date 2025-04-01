using Microsoft.AspNetCore.Identity;

namespace MegaMonster.Services.Auth.Core.User;

public class Users : IdentityUser
{
    public string Login { get; set; }
    public string Role { get; set; }
    public bool IsBan { get; set; } = false;
}