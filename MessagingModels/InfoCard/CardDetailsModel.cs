namespace MessagingModels.InfoCard;

public class CardDetailsModel(int ticketId)
{
    public int TicketId { get; set; } = ticketId;
}
