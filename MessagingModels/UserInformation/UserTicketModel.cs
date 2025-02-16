namespace MessagingModels.UserInformation;

public class UserTicketModel(Guid userId, string userName)
{
    public Guid UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
}