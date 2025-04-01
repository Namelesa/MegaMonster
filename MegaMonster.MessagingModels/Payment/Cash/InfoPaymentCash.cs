namespace MegaMonster.MessagingModels.Payment.Cash;

public class InfoPaymentCash(Guid orderId, string userName, double sum, int count)
{
    public Guid OrderId { get; set; } = orderId;
    public string UserName { get; set; } = userName;
    public double Sum { get; set; } = sum;
    public int Count { get; set; } = count;
}