namespace MegaMonster.MessagingModels.InfoPayment;

public class InfoPaymentModel(int orderId, int count, string userName, double sum)
{
    public int OrderId { get; set; } = orderId;
    public string UserName { get; set; } = userName;
    public double Sum { get; set; } = sum;
    public int Count { get; set; } = count;
    public string Action { get; set; } = Wc.Wc.PaymentActionPay;
}