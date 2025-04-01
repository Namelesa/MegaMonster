namespace MegaMonster.MessagingModels.EditUserMessage;

public class UserEditMessage(string newLogin, string oldLogin, string email, string userName, string phoneNumber, string newEmail, string newUserName, string newPhoneNumber)
{
    public string NewLogin { get; set; } = newLogin;
    public string OldLogin { get; set; } = oldLogin;
    public string Email { get; set; } = email;
    public string UserName { get; set; } = userName;
    public string PhoneNumber { get; set; } = phoneNumber;
    public string NewEmail { get; set; } = newEmail;
    public string NewUserName { get; set; } = newUserName;
    public string NewPhoneNumber { get; set; } = newPhoneNumber;
}