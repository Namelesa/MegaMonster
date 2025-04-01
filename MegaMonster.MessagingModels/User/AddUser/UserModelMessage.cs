namespace MegaMonster.MessagingModels.User.AddUser;

public class UserModelMessage(string login, string userName, string email, string password, string phoneNumber, string role)
{
    public string Login { get; set; } = login;
    public string UserName { get; set; } = userName;
    public string NormalizedUserName { get; set; } = userName.ToUpper();
    public string Email { get; set; } = email;
    public string NormalizedEmail { get; set; } = email.ToUpper();
    public string Password { get; set; } = password;
    public string PhoneNumber { get; set; } = phoneNumber;
    public string Role { get; set; } = role;
}