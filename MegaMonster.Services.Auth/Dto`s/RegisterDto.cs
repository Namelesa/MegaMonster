using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Auth.Dto_s;

public class RegisterDto
{
    [EmailAddress]
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Login { get; set; }
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }
    
    [DataType(DataType.Password)]
    public string Password { get; set; }
}