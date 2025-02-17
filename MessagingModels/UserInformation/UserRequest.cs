namespace MessagingModels.UserInformation;

public class UserRequest(string login)
{
    public string Login { get; set; } = login;
}