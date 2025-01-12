namespace MegaMonster.Services.Favors.Models;

public class Ticket(string userName, string userType, string price) : BaseModel
{
    public string UserName { get; set; } = userName;
    public Guid UserId { get; set; }
    public string UserType { get; set; } = userType;
    public string Price { get; set; } = price;
}