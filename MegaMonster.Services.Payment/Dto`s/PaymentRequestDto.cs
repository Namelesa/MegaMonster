namespace MegaMonster.Services.Payment.Dto_s;

public class PaymentRequestDto
{
    public string OrderId { get; set; }
    public string UserName { get; set; }
    public string TicketType { get; set; }
    public decimal Sum { get; set; }
    public int Count { get; set; }
}