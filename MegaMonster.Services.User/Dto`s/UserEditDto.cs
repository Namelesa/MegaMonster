using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.User.Dto_s;

public class UserEditDto
{
    public string UserName { get; set; }
    public string Login { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [Phone] public string PhoneNumber { get; set; }
}