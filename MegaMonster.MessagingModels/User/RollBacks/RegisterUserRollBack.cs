namespace MegaMonster.MessagingModels.User.RollBacks;

public class RegisterUserRollBack(string login)
{
    public string Login { get; set; } = login;
}