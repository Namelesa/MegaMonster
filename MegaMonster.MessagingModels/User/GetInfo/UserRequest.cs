namespace MegaMonster.MessagingModels.User.GetInfo;

public class UserRequest(string login)
{
    public string Login { get; set; } = login;
}