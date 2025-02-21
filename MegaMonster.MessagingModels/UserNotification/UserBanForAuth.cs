namespace MegaMonster.MessagingModels.UserNotification;

public class UserBanForAuth(string email)
{
    public string Email { get; set; } = email;
}