using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Data;
using MegaMonster.Services.Favors.Dto_s;
using MegaMonster.Services.Favors.Models;
using MegaMonster.Services.Favors.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/Favors")]
public class TicketController(AppDbContext db, TicketService ticketService) : ControllerBase
{
    // Get Requests //
    [HttpGet("tickets/userType/{type}")]
    public async Task<IActionResult> GetTicketByUserStatus(string type)
    {
        var tickets = await db.Tickets
            .Where(t => t.UserType == type)
            .ToListAsync();

        if (tickets.Any())
        {
            return Ok(tickets);
        }

        return NotFound($"No tickets found for this user type: {type}");
    }
    
    [HttpGet("tickets/configs")]
    public async Task<IActionResult> GetTicketsConfig()
    {
        var configs = await db.Configurations.ToListAsync();
        if (configs.Any())
        {
            return Ok(configs);
        }

        return NotFound("Not found tickets configurations");
    }
    // Post Requests //
    [HttpPost("ticket/buy")]
    public async Task<IActionResult> BuyTicket([Required] TicketDto ticketDto)
    {
        try
        {
            var config = await ticketService.GetConfigurationAsync(ticketDto.UserType);
            
            var ticket = new Ticket(ticketDto.UserType, ticketDto.DateTimeStart, config)
            {
                // add user info
            };
            
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            return Ok(new { Message = "Ticket successfully purchased", TicketId = ticket.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { Message = "An unexpected error occurred while buying the ticket" });
        }
    }
    
    [HttpPost("ticket/addConfiguration")]
    public async Task<IActionResult> AddConfiguration([Required][FromBody] TicketConfigurationDto configDto)
    {
        var config = new TicketConfiguration
        {
            UserType = configDto.UserType,
            Price = configDto.Price,
            DurationInHours = configDto.DurationInHours
        };
        try
        {
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            return Ok("Configuration added successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Configuration not added.");
        }
    }
    
    // Put Requests //
    [HttpPut("ticket/edit/Configuration")]
    public async Task<IActionResult> EditConfiguration([Required] string userType, [Required] TicketConfigurationDto configurationDto)
    {
        try
        {
            var currentConfig = db.Configurations.FirstOrDefault(u => u.UserType == userType);
            if (currentConfig != null)
            {
                currentConfig.UserType = configurationDto.UserType;
                currentConfig.Price = configurationDto.Price;
                currentConfig.DurationInHours = configurationDto.DurationInHours;
                db.Configurations.Update(currentConfig);
                await db.SaveChangesAsync();
                return Ok($"Successfully edit ticket configuration with type {currentConfig.UserType} to  {userType}");
            }
            return NotFound($"Not found category with name = {userType}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Category with name '{configurationDto.UserType}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Delete Requests //
    [HttpDelete("ticket/delete/Configuration")]
    public async Task<IActionResult> DeleteConfiguration([Required] string userType)
    {
        try
        {
            var currentConfig = db.Configurations.FirstOrDefault(u => u.UserType == userType);
            if (currentConfig != null)
            {
                db.Configurations.Remove(currentConfig);
                await db.SaveChangesAsync();
                return Ok($"Successfully delete ticket configuration with type {currentConfig.UserType}");
            }
            return NotFound($"Not found ticket config with type = {userType}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Ticket config with name '{userType}' already deleted");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    [HttpDelete("ticket/delete")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        try
        {
            var currentTicket = await db.Tickets.FindAsync(id);
            if (currentTicket != null)
            {
                db.Tickets.Remove(currentTicket);
                await db.SaveChangesAsync();
                return Ok("Successfully delete ticket");
            }
            return NotFound("Not found ticket");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Ticket already deleted");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }

}