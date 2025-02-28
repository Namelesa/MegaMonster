using MassTransit;
using MegaMonster.Services.Payment.Infrastructure.Service;
using MegaMonster.Services.Payment.WebApi.Dto_s;
using MegaMonster.MessagingModels.InfoPayment;
using MegaMonster.Services.Payment.Application.Services;
using MegaMonster.Services.Payment.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MegaMonster.Services.Payment.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/payment")]
public class PaymentController(PaymentService paymentService, IPublishEndpoint publishEndpoint, PaymentServiceRepository paymentServiceRepository) : ControllerBase
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
                request.Sum,
                request.Count,
                request.Action
            );

            return Ok(new { url = paymentUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }
    
    [HttpPost("cancel")]
    public async Task<IActionResult> CanceledPayment([FromBody] CancelPaymentRequest request)
    {
        if (request.OrderId == Guid.Empty)
            return BadRequest("Invalid orderId");

        var result = await paymentService.CancelPaymentAsync(request.OrderId);
        return result ? Ok("Cancel payment") : BadRequest("Errors");
    }
    
    [HttpPost("result")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> HandlePaymentResult()
    {
        try
        {
            var formData = Request.Form;
            Console.WriteLine($"Form data received: {JsonConvert.SerializeObject(formData)}");

            var requestDictionary = formData.ToDictionary(key => key.Key, key => key.Value.ToString());

            var isSuccess = await paymentService.HandlePaymentResultAsync(requestDictionary);

            if (isSuccess.isSuccess)
            {
                var publishCardModel = new InfoForCardPayment(Guid.Parse(isSuccess.orderId), isSuccess.transactionId);
                await publishEndpoint.Publish(publishCardModel);
                return Redirect("https://localhost/");
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
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpdatePaymentStatusForCash(Guid orderId)
    {
        var payment = await paymentServiceRepository.GetPaymentByOrderId(orderId);
        if (!payment.Success) return BadRequest(payment.Message);
        
        if (payment.Data == null) return BadRequest(payment.Message);
        payment.Data.Status = PaymentSettings.IsSuccess;
        
        var result = await paymentServiceRepository.UpdatePayment(payment.Data);
        return result.Success ? Ok("Status was changed") : BadRequest(result.Message);
    }
}