using System.Text.Json.Serialization;

namespace MegaMonster.Services.Card.Models;

public class OrderDetails : BaseModel
{
    public string Bill { get; set; }
    public int TicketId { get; set; }

    public int OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; }
}
