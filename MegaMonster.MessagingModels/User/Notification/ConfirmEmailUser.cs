namespace MegaMonster.MessagingModels.User.Notification;

public class ConfirmEmailUser(string login)
{
    public string Login { get; set; } = login;
}