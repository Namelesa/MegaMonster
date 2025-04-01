using MegaMonster.Services.Card.Core.Order;
using MegaMonster.Services.Card.Core.OrderDetail;

namespace MegaMonster.Services.Card.WebApi.Card;

public class Card
{
    public IEnumerable<Core.Order.Order> OrderCard { get; set; }
    public IEnumerable<Core.OrderDetail.OrderDetails> OrderDetailsCard { get; set; }
}