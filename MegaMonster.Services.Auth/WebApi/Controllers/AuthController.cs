using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Auth.Application.Interfaces;
using MegaMonster.Services.Auth.Core.Models;
using MegaMonster.Services.Auth.WebApi.Dto_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Auth.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IRegisterRepository registerRepository, ILoginRepository loginRepository) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }

        bool checkLoginAndEmail = await registerRepository.CheckLoginAndEmail(registerDto.Login, registerDto.Email);
        if (!checkLoginAndEmail)
        {
            return Conflict(new { Error = "User with the same email or login already exists." });
        }
        
        var user = new Users
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName,
            Login = registerDto.Login,
            PhoneNumber = registerDto.PhoneNumber,
            NormalizedEmail = registerDto.Email.ToUpper(),
            NormalizedUserName = registerDto.UserName.ToUpper(),
        };
        
        user.PasswordHash = await registerRepository.HashPassword(registerDto.Password, user);
        bool result = await registerRepository.RegisterUser(user);
        return result ? Ok(new { Message = "User registered successfully." }) : StatusCode(500, new { Error = "An unexpected error occurred during registration." });
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([Required, FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }
        
        bool checkLoginAndEmail = await loginRepository.CheckLoginAndEmail(loginDto.Login, loginDto.Email);
        if(checkLoginAndEmail) return NotFound($"User with login '{loginDto.Login}' not found.");

        var user = await loginRepository.FindUser(loginDto.Login);
        var token = await loginRepository.Auth(user, loginDto.Password);
        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet("test")]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok("Hello");
    }
}
