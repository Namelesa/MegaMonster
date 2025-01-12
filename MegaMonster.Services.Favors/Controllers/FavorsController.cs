using MegaMonster.Services.Favors.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavorsController(AppDbContext _db) : ControllerBase
{
    [HttpGet("getCategories")]
    public async Task<IActionResult> GetCategories()
    {
        var favors = await _db.Categories.ToArrayAsync();
        return Ok(favors);
    }
}