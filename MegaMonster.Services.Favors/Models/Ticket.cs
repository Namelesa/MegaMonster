using MegaMonster.Services.Favors.Service;

namespace MegaMonster.Services.Favors.Models;

public class Ticket : BaseModel
{
    public string? UserName { get; set; }
    public Guid? UserId { get; set; }
    public string UserType { get; set; }
    public double Price { get; set; }
    public DateTime? DateTimeStart { get; set; }
    public DateTime? DateTimeEnd { get; set; }
    
    public Ticket(string userType, DateTime startTime, TicketConfiguration config)
    {
        UserType = userType;
        Price = config.Price;
        DateTimeStart = startTime;
        DateTimeEnd = startTime.AddHours(config.DurationInHours);
    }
    public Ticket() { }
}