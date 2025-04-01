using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.User.WebApi.User;

public class UserDto
{
    public string UserName { get; set; }
    public string Login { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [Phone] public string PhoneNumber { get; set; }

    public string PasswordHash { get; set; }
}