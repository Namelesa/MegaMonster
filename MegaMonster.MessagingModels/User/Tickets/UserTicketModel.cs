namespace MegaMonster.MessagingModels.User.Tickets;

public class UserTicketModel(string userId, string userName)
{
    public string UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
}