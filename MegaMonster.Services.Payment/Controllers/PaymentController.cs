using MegaMonster.Services.Payment.Dto_s;
using MegaMonster.Services.Payment.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MegaMonster.Services.Payment.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController(PaymentService paymentService) : ControllerBase
{
    [HttpPost("createPayment")]
    public async Task<IActionResult> CreatePayment([FromForm] PaymentRequestDto request)
    {
        if (request.Sum <= 0 || request.Count <= 0)
        {
            return BadRequest(new { message = "Sum and count must be > 0" });
        }

        try
        {
            var paymentUrl = await paymentService.CreatePaymentAsync(
                request.OrderId,
                request.UserName,
                request.TicketType,
                request.Sum,
                request.Count
            );

            return Ok(new { url = paymentUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }
    
    [HttpPost("result")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> HandlePaymentResult()
    {
        try
        {
            var requestDictionary = Request.Form.ToDictionary(key => key.Key, key => key.Value.ToString());
            
            Console.WriteLine($"Received request data: {JsonConvert.SerializeObject(requestDictionary)}");

            bool isSuccess = await paymentService.HandlePaymentResultAsync(requestDictionary);

            if (isSuccess)
            {
                return Redirect("https://localhost:7215/swagger/index.html");
            }
            else
            {
                return BadRequest(new { message = "Can not update status" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }
}
