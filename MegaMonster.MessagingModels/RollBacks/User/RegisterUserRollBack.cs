namespace MegaMonster.MessagingModels.RollBacks.User;

public class RegisterUserRollBack(string login)
{
    public string Login { get; set; } = login;
}