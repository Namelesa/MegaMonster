using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Data;
using MegaMonster.Services.Favors.Dto_s;
using MegaMonster.Services.Favors.Models;
using MegaMonster.Services.Favors.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/Favors")]
public class TicketController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("tickets/userType/{type}")]
    public async Task<IActionResult> GetTicketByUserStatus(string type)
    {
        var tickets = await db.Tickets
            .Where(t => t.UserType.ToString() == type)
            .ToListAsync();

        if (tickets.Any())
        {
            return Ok(tickets);
        }

        return NotFound($"No tickets found for this user type: {type}");
    }
    // Post Requests //
    [HttpPost("ticket/buy")]
    public async Task<IActionResult> BuyTicket([Required] TicketDto ticketDto)
    {
        try
        {
            if (!Enum.TryParse<UserTypeEnum>(ticketDto.UserType, true, out var userType))
            {
                return BadRequest($"Invalid UserType: {ticketDto.UserType}. Allowed values are: {string.Join(", ", Enum.GetNames(typeof(UserTypeEnum)))}");
            }

            var ticket = new Ticket(userType)
            {
                DateTimeStart = ticketDto.DateTimeStart
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return Ok("Successfully bought ticket");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Error with bought ticket");
        }
    }
    
    // Put Requests //
    [HttpPut("ticket/edit/id/{id}")]
    public async Task<IActionResult> EditCategory(string currentName, [Required]string newName)
    {
        try
        {
            var currentCategory = db.Categories.FirstOrDefault(u => u.Name == currentName);
            if (currentCategory != null)
            {
                currentCategory.Name = newName;
                db.Categories.Update(currentCategory);
                await db.SaveChangesAsync();
                return Ok($"Successfully edit category name {currentName} to  {newName}");
            }
            return NotFound($"Not found category with name = {currentName}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Category with name '{newName}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }

}