using MegaMonster.Services.Notification.Application.Validator;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Notification.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotification notificationService, UserValidator userValidator, BillUserValidator billUserValidator) : ControllerBase
{
    [HttpPost("confirmRegister")]
    public async Task<IActionResult> SendConfirmEmail([FromBody]UserDto userDto)
    {
        var validateResult = await userValidator.ValidateAsync(userDto);
        if (!validateResult.IsValid)
        {
            return BadRequest(validateResult.Errors[0].ErrorMessage);
        }
        var result = await notificationService.SendConfirmEmailAsync(userDto);
        return result ? Ok("Send confirm email") : BadRequest("Template file not found.");
    }
    
    [Authorize]
    [HttpPost("bill")]
    public async Task<IActionResult> SendBillEmail([FromBody]BillUserDto userDto)
    {
        var validateResult = await billUserValidator.ValidateAsync(userDto);
        if (!validateResult.IsValid)
        {
            return BadRequest(validateResult.Errors[0].ErrorMessage);
        }
        var result = await notificationService.SendBillEmailAsync(userDto);
        return result ? Ok("Send Bill email") : BadRequest("Template file not found.");
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("ban")]
    public async Task<IActionResult> SendBanEmail([FromBody]UserDto userDto, string reason)
    {
        var validateResult = await userValidator.ValidateAsync(userDto);
        if (!validateResult.IsValid)
        {
            return BadRequest(validateResult.Errors[0].ErrorMessage);
        }
        var result = await notificationService.SendBanEmailAsync(userDto, reason);
        return result ? Ok("Send Ban email") : BadRequest("Template file not found.");
    }
    
    [Authorize]
    [HttpPost("news")]
    public async Task<IActionResult> SendNewsEmail([FromBody]UserDto userDto)
    {
        var validateResult = await userValidator.ValidateAsync(userDto);
        if (!validateResult.IsValid)
        {
            return BadRequest(validateResult.Errors[0].ErrorMessage);
        }
        var result = await notificationService.SendNewsEmailAsync(userDto);
        return result ? Ok("Send email with news") : BadRequest("Template file not found.");
    }
}