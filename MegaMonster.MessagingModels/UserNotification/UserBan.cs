namespace MegaMonster.MessagingModels.UserNotification;

public class UserBan(string userName, string email, string reason) : BaseNotificationMessage(userName, email)
{ 
    public string Reason { get; set; } = reason;
}