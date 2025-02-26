namespace MegaMonster.MessagingModels.InfoPayment;

public class InfoBillModel(string userName, string paymentType, string paymentStatus, double sum, Guid orderId, string email)
{
    public string UserName { get; set; } = userName;
    public Guid OrderId { get; set; } = orderId;
    public string PaymentType { get; set; } = paymentType;
    public string PaymentStatus { get; set; } = paymentStatus;
    public double Sum { get; set; } = sum;
    public string Email { get; set; } = email;
    public List<int> TicketsIds { get; set; }
}