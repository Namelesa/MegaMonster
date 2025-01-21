namespace MegaMonster.Services.Card.Dto_s;

public class OrderDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public double Sum { get; set; }
    
    public string Bill { get; set; }
    public int TicketId { get; set; }
    public int OrderId { get; set; }
    
}