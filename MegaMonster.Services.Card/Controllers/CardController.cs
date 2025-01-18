using MegaMonster.Services.Card.Data;
using MegaMonster.Services.Card.Models.ViewModel;
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
        var order = await db.Orders.Where(t => t.UserId == userId).ToListAsync();
        if (order.Any())
        {
            Models.ViewModel.Card card = new Models.ViewModel.Card()
            {
                OrderCard = order,
            
            };
            return Ok(card);
        }

        return NotFound("Not found your orders");
    }
}