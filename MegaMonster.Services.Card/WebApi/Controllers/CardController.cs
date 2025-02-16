using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Card.Application.Services;
using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.WebApi.Dto_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Card.WebApi.Controllers;

[ApiController]
[Route("api/card")]
public class CardController(OrderService orderService, OrderDetailsService orderDetailsService) : ControllerBase
{
    // Get Requests //
    [Authorize]
    [HttpGet("card")]
    public async Task<IActionResult> GetCard(string userId)
    {
        var orderIds = await orderService.GetOrdersIdByUserId(userId);
        if (!orderIds.Any())
        {
            return NotFound("No orders found for this user.");
        }

        var orderDetails = await orderDetailsService.GetOrderDetails(orderIds);
        var orders = await orderService.GetOrdersByUserId(userId);

        return Ok(new ViewModel.Card
        {
            OrderCard = orders,
            OrderDetailsCard = orderDetails
        });
    }
    
    // Post Requests //
    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCard([FromBody] OrderDto orderDto)
    {
        
        if (string.IsNullOrEmpty(orderDto.UserId) || string.IsNullOrEmpty(orderDto.UserName))
        {
            return BadRequest("UserId and UserName are required.");
        }

        if (!orderDto.OrderDetails.Any())
        {
            return BadRequest("At least one order detail is required.");
        }

        if (orderDto.OrderDetails.Count > 10)
        {
            return BadRequest("You cannot add more than 10 order details.");
        }

        var order = new Order
        {
            UserId = orderDto.UserId,
            UserName = orderDto.UserName,
            Sum = orderDto.Sum
        };

        order.OrderDetails = orderDto.OrderDetails
            .Select(detailsDto => new OrderDetails
            {
                Bill = detailsDto.Bill,
                TicketId = detailsDto.TicketId,
                Order = order
            })
            .ToList();

        var result = await orderService.AddOrder(order);

        return result.Success 
            ? Ok(new { message = "Order successfully added" }) 
            : BadRequest(result.Message);
    }
    
    // Put Requests //
    [Authorize]
    [HttpPut("edit")]
    public async Task<IActionResult> EditCard([Required] int orderId, [Required] int orderDetailsId, [FromBody] OrderEditDto orderEditDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await orderService.GetAllOrder(orderId);

        if (order == null)
        {
            return NotFound($"Order with ID {orderId} not found.");
        }
        
        order.Sum = orderEditDto.Sum;

        var orderDetails = order.OrderDetails.FirstOrDefault(od => od.Id == orderDetailsId);
        if (orderDetails == null)
        {
            return NotFound("OrderDetails with ID not found for this order.");
        }

        orderDetails.Bill = orderEditDto.Bill;
        orderDetails.TicketId = orderEditDto.TicketId;
        var result = await orderService.UpdateOrder(order);
        return result.Success ? Ok("Order updated") : BadRequest(result.Message);
    }
    
    // Delete Requests //
    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCard([Required] int orderId)
    {
        var result = await orderService.DeleteById(orderId);
        return result.Success ? Ok("Order deleted") : BadRequest(result.Message);
        //send delete to ticket
    }
    
}