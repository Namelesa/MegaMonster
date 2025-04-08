using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Notification.Core.User;

public class UserDto(string userName, string email, string confirmLink = null)
{
    [Required]
    public string UserName { get; } = userName;
    
    [Required]
    public string Email { get; } = email;

    [Required] public string ConfirmLink { get; } = confirmLink;
}