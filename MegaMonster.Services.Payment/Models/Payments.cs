using MegaMonster.Services.Payment.Infrastructure;
using Newtonsoft.Json;

namespace MegaMonster.Services.Payment.Models;

public class Payments
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("order_id")]
    public string OrderId { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = PaymentSettings.IsCreated;

    [JsonProperty("user_name")]
    public string UserName { get; set; }

    [JsonProperty("ticket_type")]
    public string TicketType { get; set; }

    [JsonProperty("amount")]
    public decimal Sum { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}