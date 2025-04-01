using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Auth.Application.User;
using MegaMonster.Services.Auth.WebApi.Dto_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Auth.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }

        var result = await authService.RegisterUser(registerDto.Password, registerDto.Email, registerDto.UserName, registerDto.Login, registerDto.PhoneNumber);
        return result.Success ? Ok("Register user") : BadRequest(result.Message);
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([Required, FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }
        
        var result = await authService.LoginUser(loginDto.Password, loginDto.Email, loginDto.Login);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }
        
        Response.Cookies.Append("access_token", result.Message, new CookieOptions
        {
            HttpOnly = true, 
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });
        return Ok(new { message = "Login successful" });
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string email)
    {
        var result = await authService.ConfirmEmail(email);
        if (result.Success)
            return Ok("Email confirmed");

        return BadRequest(result.Message);
    }

}
