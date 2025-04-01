using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Payment.WebApi.Payment;

public class CancelPaymentRequest
{
    [Required] public Guid OrderId { get; set; }
}