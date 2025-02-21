namespace MegaMonster.MessagingModels.UserNotification;

public class BaseNotificationMessage(string userName, string email)
{
    public string UserName { get; set; } = userName;
    public string Email { get; set; } = email;
}