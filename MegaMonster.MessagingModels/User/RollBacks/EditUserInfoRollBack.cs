namespace MegaMonster.MessagingModels.User.RollBacks;

public class EditUserInfoRollBack
{
    public string Login { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string NewLogin { get; set; }
}