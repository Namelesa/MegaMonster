using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Payment.WebApi.Dto_s;

public class CancelPaymentRequest
{
    [Required] public Guid OrderId { get; set; }
}