namespace MegaMonster.Services.Card.Models;

public class OrderDetails : BaseModel
{
    public string Bill { get; set; }
    public int TicketId { get; set; }
}