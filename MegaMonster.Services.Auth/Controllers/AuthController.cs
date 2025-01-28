using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Auth.Data;
using MegaMonster.Services.Auth.Dto_s;
using MegaMonster.Services.Auth.JWT;
using MegaMonster.Services.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, PasswordHasher<Users> passwordHasher, JwtService jwtService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }
        
        if (await db.Users.AnyAsync(u => u.Email == registerDto.Email || u.Login == registerDto.Login))
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
        
        user.PasswordHash = passwordHasher.HashPassword(user, registerDto.Password);

        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok(new { Message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { Error = "An unexpected error occurred during registration." });
        }
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([Required, FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
        }

        
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);
        if (user == null) return NotFound($"User with login '{loginDto.Login}' not found.");

        if (user.PasswordHash != null)
        {
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (verificationResult != PasswordVerificationResult.Success)
            {
                return Unauthorized(new { Error = "Invalid password." });
            }
        }
        var result = await jwtService.Authenticate(loginDto);
        if (result != "Null")
        {
            return Ok(new
            {
                Message = $"Welcome back, {user.UserName}!" +
                          $"Token {result}"
            });
        }

        return BadRequest("Error");
    }

    [Authorize]
    [HttpGet("test")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await db.Users.ToListAsync();
        return Ok(users);
    }
}
