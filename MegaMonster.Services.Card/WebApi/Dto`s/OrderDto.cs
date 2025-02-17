namespace MegaMonster.Services.Card.WebApi.Dto_s;

public class OrderDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public double Sum { get; set; }
    public List<OrderDetailsDto> OrderDetails { get; set; }
}