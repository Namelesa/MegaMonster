namespace MegaMonster.MessagingModels.EditUserMessage;

public class UserEditMessage(string oldLogin, string newLogin, string email, string userName, string phoneNumber)
{
    public string NewLogin { get; set; } = newLogin;
    public string OldLogin { get; set; } = oldLogin;
    public string Email { get; set; } = email;
    public string UserName { get; set; } = userName;
    public string PhoneNumber { get; set; } = phoneNumber;
}