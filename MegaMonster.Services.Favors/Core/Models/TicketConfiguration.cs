namespace MegaMonster.Services.Favors.Core.Models;

public class TicketConfiguration : BaseModel
{
    public string UserType { get; set; }
    public double Price { get; set; }
    public int DurationInHours { get; set; }
}
