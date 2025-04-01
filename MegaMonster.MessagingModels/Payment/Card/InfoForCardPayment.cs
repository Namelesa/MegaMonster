namespace MegaMonster.MessagingModels.Payment.Card;

public class InfoForCardPayment(Guid orderId, string bill)
{
    public Guid OrderId { get; set; } = orderId;
    public string Bill { get; set; } = bill;
}