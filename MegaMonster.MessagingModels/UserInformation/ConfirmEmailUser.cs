namespace MegaMonster.MessagingModels.UserInformation;

public class ConfirmEmailUser(string login)
{
    public string Login { get; set; } = login;
}