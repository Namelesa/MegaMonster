namespace MegaMonster.MessagingModels.InfoCard;

public class CardInfoModel(Guid userId, string userName, double sum, List<CardDetailsModel> ticketDetails, string paymentType)
{
    public Guid UserId { get; set; } = userId;
    public string UserName { get; set; } = userName;
    public double Sum { get; set; } = sum;
    public string PaymentType { get; set; } = paymentType;
    public string? Bill { get; set; }

    public List<CardDetailsModel> TicketDetails { get; set; } = ticketDetails;
}