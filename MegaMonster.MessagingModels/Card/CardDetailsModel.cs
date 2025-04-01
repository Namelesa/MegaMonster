namespace MegaMonster.MessagingModels.Card;

public class CardDetailsModel(int ticketId)
{
    public int TicketId { get; set; } = ticketId;
}
