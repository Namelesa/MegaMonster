namespace MegaMonster.Services.Payment.WebApi.Dto_s;

public class PaymentRequestDto
{
    public string OrderId { get; set; }
    public string UserName { get; set; }
    public string TicketType { get; set; }
    public double Sum { get; set; }
    public int Count { get; set; }
    public string Action { get; set; }
}