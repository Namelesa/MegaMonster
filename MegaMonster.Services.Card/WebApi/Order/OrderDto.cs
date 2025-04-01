using MegaMonster.Services.Card.WebApi.OrderDetails;

namespace MegaMonster.Services.Card.WebApi.Order;

public class OrderDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public double Sum { get; set; }
    public List<OrderDetailsDto> OrderDetails { get; set; }
}