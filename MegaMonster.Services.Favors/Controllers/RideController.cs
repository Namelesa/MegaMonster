using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Data;
using MegaMonster.Services.Favors.Dto_s;
using MegaMonster.Services.Favors.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Controllers;

[ApiController]
[Route("api/Favors")]
public class RideController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("rides")]
    public async Task<IActionResult> GetCategories()
    {
        var rides = await db.Rides.ToArrayAsync();
        return Ok(rides);
    }
    
    [HttpGet("ride/id/{ridesId}")]
    public async Task<IActionResult> GetRideById(int ridesId)
    {
        var ride = await db.Rides.FindAsync(ridesId);
        if (ride != null)
        {
            return Ok(ride);
        }

        return NotFound("Not found ride with this id");
    }
    
    [HttpGet("ride/name/{name}")]
    public async Task<IActionResult> GetRideByName(string name)
    {
        var ride = await db.Rides.FirstOrDefaultAsync(u => u.Name == name);
        if (ride != null)
        {
            return Ok(ride);
        }

        return NotFound("Not found ride with this name");
    }
    
    // Post Requests //
    [HttpPost("ride/add")]
    public async Task<IActionResult> AddRide(string rideName, [Required] string categoryName, [Required] string clientStatus)
    {
        try
        {
            var ride = new Ride(rideName);
            var category = await db.Categories.FirstOrDefaultAsync(u => u.Name == categoryName);
            if (category != null)
            {
                ride.CategoryName = category;
                ride.CategoryId = category.Id;
                db.Rides.Add(ride);
                await db.SaveChangesAsync();
                return Ok(ride);
            }

            return NotFound("Not found category with this name");
        }
           
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Ride with name '{rideName}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Put Requests //
    [HttpPut("ride/edit/name/{currentName}")]
    public async Task<IActionResult> EditCategory(string currentName, [Required]RideDto rideDto)
    {
        try
        {
            var currentRide = db.Rides.FirstOrDefault(u => u.Name == currentName);
            var category = db.Categories.FirstOrDefault(u => u.Name == rideDto.CategoryName);
            
            if (currentRide != null && category != null)
            {
                currentRide.Name = rideDto.NewName;
                currentRide.CategoryName = category;
                currentRide.CategoryId = category.Id;
                db.Rides.Update(currentRide);
                await db.SaveChangesAsync();
                return Ok($"Successfully edit ride name {currentName} to  {rideDto.NewName}");
            }
            return NotFound($"Not found category with name = {rideDto.CategoryName}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return BadRequest($"Ride with name '{rideDto.NewName}' already exists.");
        }
        catch (Exception e)
        {
            return BadRequest($"Error: {e.Message}");
        }
    }
    
    // Delete Requests // 
    [HttpDelete("ride/delete/name/{rideName}")]
    public async Task<IActionResult> DeleteRide(string rideName)
    {
        var currentRide = db.Rides.FirstOrDefault(u => u.Name == rideName);
        if (currentRide != null)
        {
            db.Rides.Remove(currentRide);
            await db.SaveChangesAsync();
            return Ok($"Successfully delete ride with name {currentRide.Name}");
        }
        return NotFound("Not found ride with this name");
    }
}