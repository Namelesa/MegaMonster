using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Notification.Dto_s;

public class UserDto(string firstName, string lastName, string email, string telegram)
{
    [Required]
    public string FirstName { get; set; } = firstName;

    [Required]
    public string LastName { get; set; } = lastName;

    [Required]
    public string Email { get; set; } = email;

    [Required]
    public string Telegram { get; set; } = telegram;
}