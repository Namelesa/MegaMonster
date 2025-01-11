using MegaMonster.Services.Notification.Dto_s;
using MegaMonster.Services.Notification.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Notification.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotification notificationService) : ControllerBase
{
    [HttpPost("send_confirm_email")]
    public async Task<IActionResult> SendConfirmEmail([FromBody]UserDto userDto)
    {
        var result = await notificationService.SendConfirmEmailAsync(userDto);

        if (result)
        {
            return Ok();
        }

        return NotFound("Template file not found.");
    }
}