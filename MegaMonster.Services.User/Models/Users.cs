using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MegaMonster.Services.User.Models;

public class Users: IdentityUser
{
    public string Login { get; set; }
    [Display(Name ="Role")]
    public Guid RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Role Role { get; set; }
}