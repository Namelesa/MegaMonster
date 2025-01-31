using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.WebApi.ViewModel;

public class Card
{
    public List<Order> OrderCard { get; set; }
    public List<OrderDetails> OrderDetailsCard { get; set; }
}