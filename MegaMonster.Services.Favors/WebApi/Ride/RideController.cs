using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Application.Category;
using MegaMonster.Services.Favors.Application.Ride;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.Favors.WebApi.Ride;

[ApiController]
[Route("api/Favors")]
public class RideController(RideService rideService, CategoryService categoryService) : ControllerBase
{
    // Get Requests //
    [Authorize]
    [HttpGet("rides")]
    public async Task<IActionResult> GetRides()
    {
        var rides = await rideService.GetAllRides();
        return Ok(rides);
    }
    
    [Authorize]
    [HttpGet("ride/id/{rideId}")]
    public async Task<IActionResult> GetRideById(int rideId)
    {
        if (rideId <= 0) return BadRequest("Invalid ride ID.");

        var ride = await rideService.GetRideById(rideId);
        return ride is not null ? Ok(ride) : NotFound($"Category with ID {rideId} not found.");
    }
    
    [Authorize]
    [HttpGet("ride/name/{name}")]
    public async Task<IActionResult> GetRideByName(string name)
    {
        var ride = await rideService.GetRideByName(name);
        return ride is not null ? Ok(ride) : NotFound($"Ride with name '{name}' not found.");
    }
    
    // Post Requests //
    [Authorize(Roles = "Admin")]
    [HttpPost("ride/add")]
    public async Task<IActionResult> AddRide([Required] RideAddDto rideDto)
    {
        var ride = new Core.Ride.Ride(rideDto.RideName);
        
        var category = await categoryService.GetCategoryByName(rideDto.CategoryName);
        if(category == null) return NotFound("Not found category with this name");
        
        ride.CategoryId = category.Id;
        ride.ClientStatus = rideDto.Status;
        ride.Rating = rideDto.Rating;
        var result = await rideService.AddRide(ride);
        return result.Success ? Ok("Add a new ride") : BadRequest(result.Message);
    }
    
    // Put Requests //
    [Authorize(Roles = "Admin")]
    [HttpPut("ride/edit/name/{currentName}")]
    public async Task<IActionResult> EditRide(string currentName, [Required]RideDto rideDto)
    {
        var category = await categoryService.GetCategoryByName(rideDto.CategoryName);
        if (category == null) return NotFound("Not found category with this name");

        var result = await rideService.EditRide(currentName, rideDto.NewName, category.Id, rideDto.Status, rideDto.Rating);
        return result.Success ? Ok("Edit ride") : BadRequest(result.Message);
    }
    
    // Delete Requests //
    [Authorize(Roles = "Admin")]
    [HttpDelete("ride/delete/name/{rideName}")]
    public async Task<IActionResult> DeleteRide(string rideName)
    {
        var result = await rideService.DeleteRide(rideName);
        return result.Success ? Ok("Delete ride") : BadRequest(result.Message);
    }
}