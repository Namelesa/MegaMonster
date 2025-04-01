namespace MegaMonster.Services.Card.WebApi.Order;

public class OrderEditDto
{
    public double Sum { get; set; }
    
    public string Bill { get; set; }
    public string PaymentType { get; set; }
    public int TicketId { get; set; }
}