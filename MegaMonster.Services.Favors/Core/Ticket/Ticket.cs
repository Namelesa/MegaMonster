namespace MegaMonster.Services.Favors.Core.Ticket;

public class Ticket : BaseModel.BaseModel
{
    public string? UserName { get; set; }
    public Guid UserId { get; set; }
    public string UserType { get; set; }
    public double Price { get; set; }
    public DateTime? DateTimeStart { get; set; }
    public DateTime? DateTimeEnd { get; set; }
    public string PaymentType { get; set; }
    
    public Ticket(string userName, string userType, DateTime startTime, TicketConfiguration.TicketConfiguration config, Guid userId)
    {
        UserName = userName;
        UserType = userType;
        Price = config.Price;
        DateTimeStart = startTime;
        DateTimeEnd = startTime.AddHours(config.DurationInHours);
        UserId = userId;
    }
    public Ticket() { }
}