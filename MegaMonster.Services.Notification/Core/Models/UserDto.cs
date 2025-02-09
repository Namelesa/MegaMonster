using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Notification.Core.Models;

public class UserDto(string userName, string email)
{
    [Required]
    public string UserName { get; set; } = userName;
    
    [Required]
    public string Email { get; set; } = email;
}