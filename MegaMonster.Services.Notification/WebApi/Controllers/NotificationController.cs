using MegaMonster.Services.Notification.Infrastructure.Service;
using MegaMonster.Services.Notification.WebApi.Dto_s;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Notification.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotification notificationService) : ControllerBase
{
    [HttpPost("confirmRegister")]
    public async Task<IActionResult> SendConfirmEmail([FromBody]UserDto userDto)
    {
        var result = await notificationService.SendConfirmEmailAsync(userDto);
        return result ? Ok("Send confirm email") : BadRequest("Template file not found.");
    }
    
    [HttpPost("bill")]
    public async Task<IActionResult> SendBillEmail([FromBody]UserDto userDto, string url)
    {
        var result = await notificationService.SendBillEmailAsync(userDto, url);
        return result ? Ok("Send Bill email") : BadRequest("Template file not found.");
    }
    
    [HttpPost("ban")]
    public async Task<IActionResult> SendBanEmail([FromBody]UserDto userDto, string reason)
    {
        var result = await notificationService.SendBanEmailAsync(userDto, reason);
        return result ? Ok("Send Ban email") : BadRequest("Template file not found.");
    }
    
    [HttpPost("news")]
    public async Task<IActionResult> SendNewsEmail([FromBody]UserDto userDto)
    {
        var result = await notificationService.SendNewsEmailAsync(userDto);
        return result ? Ok("Send email with news") : BadRequest("Template file not found.");
    }
}