namespace MegaMonster.MessagingModels.InfoPayment;

public class InfoForCardPayment(int orderId, string bill)
{
    public int OrderId { get; set; } = orderId;
    public string Bill { get; set; } = bill;
}