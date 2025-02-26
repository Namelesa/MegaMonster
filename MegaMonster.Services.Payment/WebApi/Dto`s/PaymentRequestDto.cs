namespace MegaMonster.Services.Payment.WebApi.Dto_s;

public class PaymentRequestDto
{
    public Guid OrderId { get; set; }
    public string UserName { get; set; }
    public double Sum { get; set; }
    public int Count { get; set; }
    public string Action { get; set; }
    public string Email { get; set; }
}