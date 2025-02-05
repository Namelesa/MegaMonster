using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.WebApi.ViewModel;

public class Card
{
    public IEnumerable<Order> OrderCard { get; set; }
    public IEnumerable<OrderDetails> OrderDetailsCard { get; set; }
}