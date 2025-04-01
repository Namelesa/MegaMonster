namespace MegaMonster.MessagingModels.User.Notification.Ban;

public class UserBanForAuth(string email)
{
    public string Email { get; set; } = email;
}