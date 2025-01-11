using MegaMonster.Services.Notification.Dto_s;
using MegaMonster.Services.Notification.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Notification.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotification notificationService) : ControllerBase
{
    [HttpPost("confirmRegister")]
    public async Task<IActionResult> SendConfirmEmail([FromBody]UserDto userDto)
    {
        var result = await notificationService.SendConfirmEmailAsync(userDto);

        if (result)
        {
            return Ok();
        }

        return NotFound("Template file not found.");
    }
    
    [HttpPost("bill")]
    public async Task<IActionResult> SendBillEmail([FromBody]UserDto userDto, string url)
    {
        var result = await notificationService.SendBillEmailAsync(userDto, url);

        if (result)
        {
            return Ok();
        }

        return NotFound("Template file not found.");
    }
    
    [HttpPost("ban")]
    public async Task<IActionResult> SendBanEmail([FromBody]UserDto userDto, string reason)
    {
        var result = await notificationService.SendBanEmailAsync(userDto, reason);

        if (result)
        {
            return Ok();
        }

        return NotFound("Template file not found.");
    }
    
    [HttpPost("news")]
    public async Task<IActionResult> SendNewsEmail([FromBody]UserDto userDto)
    {
        var result = await notificationService.SendNewsEmailAsync(userDto);

        if (result)
        {
            return Ok();
        }

        return NotFound("Template file not found.");
    }
}