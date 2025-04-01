namespace MegaMonster.MessagingModels.User.Notification;

public class UserNotificationBase(string userName, string email, string confirmationLink = null) : BaseNotificationMessage(userName, email)
{
    public string ConfirmationLink { get; set; } = confirmationLink;
}