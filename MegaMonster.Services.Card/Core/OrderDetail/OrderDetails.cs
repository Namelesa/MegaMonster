using System.Text.Json.Serialization;
using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Core.OrderDetail;

public class OrderDetails : BaseModel
{
    public int TicketId { get; set; }

    public Guid OrderId { get; set; }
    [JsonIgnore]
    public Order.Order Order { get; set; }
}
