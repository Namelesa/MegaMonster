using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MegaMonster.Services.Favors.Application.Ticket;
using MegaMonster.Services.Favors.Core.TicketConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Favors.WebApi.Ticket;

[ApiController]
[Route("api/Favors")]
public class TicketController(TicketsService ticketsService) : ControllerBase
{
    // Get Requests //
    [Authorize]
    [HttpGet("tickets/userType/{type}")]
    public async Task<IActionResult> GetTicketByUserStatus(string type)
    {
        var tickets = await ticketsService.GetAllTicketsByStatus(type);
        if (tickets.Any())
        {
            return Ok(tickets);
        }
        return NotFound($"No tickets found for this user type: {type}");
    }

    [Authorize]
    [HttpGet("tickets/configs")]
    public async Task<IActionResult> GetTicketsConfig()
    {
        var configurations = await ticketsService.GetAllTicketConfigurations();
        return Ok(configurations);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("tickets/getTicketById/{id}")]
    public async Task<IActionResult> GetTicketById(int id) => Ok(await ticketsService.GetTicketById(id));
    
    // Post Requests //
    [Authorize]
    [HttpPost("ticket/buy")]
    public async Task<IActionResult> BuyTicket([Required] List<TicketDto> ticketDtos, [Required]string paymentType)
    {
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (userName == null || userId == null) 
            return BadRequest("UserName can not be null");

        var tickets = new List<Core.Ticket.Ticket>();
        foreach (var ticketDto in ticketDtos)
        {
            var config = await ticketsService.GetConfigurationForUser(ticketDto.UserType);
            tickets.Add(new Core.Ticket.Ticket(userName, ticketDto.UserType, ticketDto.DateTimeStart, config, Guid.Parse(userId)));
        }

        var result = await ticketsService.BuyTickets(tickets, userName, Guid.Parse(userId), paymentType);
        return result.Success 
            ? Ok("Message = Tickets successfully purchased") 
            : BadRequest(result.Message);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("ticket/addConfiguration")]
    public async Task<IActionResult> AddConfiguration([Required][FromBody] TicketConfigurationDto configDto)
    {
        var config = new TicketConfiguration
        {
            UserType = configDto.UserType,
            Price = configDto.Price,
            DurationInHours = configDto.DurationInHours
        };
        var result = await ticketsService.AddConfiguration(config);
        return result.Success ? Ok("Add configuration") : BadRequest(result.Message);
    }
    
    // Put Requests //
    [Authorize(Roles = "Admin")]
    [HttpPut("ticket/edit/Configuration")]
    public async Task<IActionResult> EditConfiguration([Required] string userType, [Required] TicketConfigurationDto configurationDto)
    {
        var result = await ticketsService.EditConfiguration(userType, configurationDto.Price, configurationDto.DurationInHours, configurationDto.UserType);
        return result.Success ? Ok("Edit configuration") : BadRequest(result.Message);
    }
    
    // Delete Requests //
    [Authorize(Roles = "Admin")]
    [HttpDelete("ticket/delete/Configuration")]
    public async Task<IActionResult> DeleteConfiguration([Required] string userType)
    {
        var result = await ticketsService.DeleteTicketConfiguration(userType);
        return result.Success ? Ok("Delete config for this type") : BadRequest(result.Message);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("ticket/delete")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var result = await ticketsService.DeleteTicket(id);
        return result.Success ? Ok("Delete ticket") : BadRequest(result.Message);
    }

}