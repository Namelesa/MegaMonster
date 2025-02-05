using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Application.Services;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.WebApi.Dto_s;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Favors.WebApi.Controllers;

[ApiController]
[Route("api/Favors")]
public class TicketController(TicketsService ticketsService) : ControllerBase
{
    // Get Requests //
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

    [HttpGet("tickets/configs")]
    public async Task<IActionResult> GetTicketsConfig()
    {
        var configurations = await ticketsService.GetAllTicketConfigurations();
        return Ok(configurations);
    }
    
    // Post Requests //
    [HttpPost("ticket/buy")]
    public async Task<IActionResult> BuyTicket([Required] TicketDto ticketDto)
    {
        var config = await ticketsService.GetConfigurationForUser(ticketDto.UserType);
            
        var ticket = new Ticket(ticketDto.UserName, ticketDto.UserType, ticketDto.DateTimeStart, config, ticketDto.UserId)
        { 
            // add user info
        };

        var result = await ticketsService.BuyTicket(ticket);
        return result.Success ? Ok($"Message = Ticket successfully purchased, TicketId = {ticket.Id }") : BadRequest(result.Message); 
            
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
        var result = await ticketsService.AddConfiguration(config);
        return result.Success ? Ok("Add configuration") : BadRequest(result.Message);
    }
    
    // Put Requests //
    [HttpPut("ticket/edit/Configuration")]
    public async Task<IActionResult> EditConfiguration([Required] string userType, [Required] TicketConfigurationDto configurationDto)
    {
        var result = await ticketsService.EditConfiguration(userType, configurationDto.Price, configurationDto.DurationInHours, configurationDto.UserType);
        return result.Success ? Ok("Edit configuration") : BadRequest(result.Message);
    }
    
    // Delete Requests //
    [HttpDelete("ticket/delete/Configuration")]
    public async Task<IActionResult> DeleteConfiguration([Required] string userType)
    {
        var result = await ticketsService.DeleteTicketConfiguration(userType);
        return result.Success ? Ok("Delete config for this type") : BadRequest(result.Message);
    }
    
    [HttpDelete("ticket/delete")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var result = await ticketsService.DeleteTicket(id);
        return result.Success ? Ok("Delete ticket") : BadRequest(result.Message);
    }

}