using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Auth.Application.Services;
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
        return result.Success ? Ok(result.Message) : BadRequest(result.Message);
    }
}
