using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Core.OrderDetail;

namespace MegaMonster.Services.Card.Core.Order;

public class Order : BaseModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string PaymentType { get; set; }
    public string? Bill { get; set; }
    public string Status { get; set; } = Wc.CreatedStatus;
    public double Sum { get; set; }
    
    public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
}
