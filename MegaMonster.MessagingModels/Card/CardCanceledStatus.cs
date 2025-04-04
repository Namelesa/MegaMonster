namespace MegaMonster.MessagingModels.Card;

public class CardCanceledStatus(Guid orderId, string status)
{
    public Guid OrderId { get; set; } = orderId;
    public string Status { get; set; } = status;
}