using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.TicketConfiguration;

public class TicketConfiguration : BaseModel
{
    public string UserType { get; set; }
    public double Price { get; set; }
    public int DurationInHours { get; set; }
}
