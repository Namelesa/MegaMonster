namespace MegaMonster.MessagingModels.InfoPayment;

public class InfoForCardPayment(Guid orderId, string bill)
{
    public Guid OrderId { get; set; } = orderId;
    public string Bill { get; set; } = bill;
}