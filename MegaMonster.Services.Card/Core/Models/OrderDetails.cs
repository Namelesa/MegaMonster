using System.Text.Json.Serialization;

namespace MegaMonster.Services.Card.Core.Models;

public class OrderDetails : BaseModel
{
    public int TicketId { get; set; }

    public Guid OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; }
}
