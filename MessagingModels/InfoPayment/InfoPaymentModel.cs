namespace MessagingModels.InfoPayment;

public class InfoPaymentModel(int orderId, int count, string userName, string ticketType, double sum)
{
    public int OrderId { get; set; } = orderId;
    public string UserName { get; set; } = userName;
    public string TicketType { get; set; } = ticketType;
    public double Sum { get; set; } = sum;
    public int Count { get; set; } = count;
}