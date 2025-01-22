using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Card.Data;
using MegaMonster.Services.Card.Dto_s;
using MegaMonster.Services.Card.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Card.Controllers;

[ApiController]
[Route("api/card")]
public class CardController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("card")]
    public async Task<IActionResult> GetCard(string userId)
    {
        var orders = await db.Orders.Where(t => t.UserId == userId).ToListAsync();

        if (orders.Any())
        {
            var orderIds = orders.Select(o => o.Id).ToList();
            var orderDetails = await db.OrdersDetails
                .Where(od => orderIds.Contains(od.OrderId))
                .ToListAsync();
            Models.ViewModel.Card card = new Models.ViewModel.Card
            {
                OrderCard = orders,
                OrderDetailsCard = orderDetails
            };

            return Ok(card);
        }

        return NotFound("Not found your orders");
    }
    
    // Post Requests //
    [HttpPost("add")]
    public async Task<IActionResult> AddCard([FromBody] OrderDto orderDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrEmpty(orderDto.UserId) || string.IsNullOrEmpty(orderDto.UserName))
        {
            return BadRequest("UserId and UserName are required.");
        }
        
        var order = new Order
        {
            UserId = orderDto.UserId,
            UserName = orderDto.UserName,
            Sum = orderDto.Sum
        };

        var orderDetails = new OrderDetails
        {
            Bill = orderDto.Bill,
            TicketId = orderDto.TicketId,
            Order = order 
        };

        order.OrderDetails.Add(orderDetails);

        var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            db.Orders.Add(order); 
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Order successfully added");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error with adding: {ex.Message}");
        }
    }
    
    // Put Requests //
    [HttpPut("edit")]
    public async Task<IActionResult> EditCard([Required] int orderId, [Required] int orderDetailsId, [FromBody] OrderEditDto orderEditDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var order = await db.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound($"Order with ID {orderId} not found.");
        }
        
        order.Sum = orderEditDto.Sum;

        var orderDetails = order.OrderDetails.FirstOrDefault(od => od.Id == orderDetailsId);
        if (orderDetails != null)
        {
            orderDetails.Bill = orderEditDto.Bill;
            orderDetails.TicketId = orderEditDto.TicketId;
        }
        else
        {
            return NotFound("OrderDetails with ID not found for this order.");
        }
        try
        {
            db.Orders.Update(order);
            await db.SaveChangesAsync();
            return Ok("Order successfully edited.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error with editing: {ex.Message}");
        }
    }
    
    // Delete Requests //
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCard([Required] int orderId)
    {
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return NotFound("Error with deleting item");
        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return Ok("Removing successfully");
        //send delete to ticket
    }
    
}