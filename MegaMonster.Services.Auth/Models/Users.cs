using Microsoft.AspNetCore.Identity;

namespace MegaMonster.Services.Auth.Models;

public class Users : IdentityUser
{
    public string Login { get; set; }
}