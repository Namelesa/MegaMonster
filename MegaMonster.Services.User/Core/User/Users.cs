using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MegaMonster.Services.User.Core.User;

public class Users: IdentityUser
{
    public string Login { get; set; }
    [Display(Name ="Role")]
    public Guid RoleId { get; init; }
    [ForeignKey("RoleId")]
    public Role.Role Role { get; init; }
    
    public Users(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException("login cannot be empty.");
        }

        if (login.Length < 6)
        {
            throw new ArgumentException("login name must be at least 5 characters long.");
        }

        Login = login;
    }
}