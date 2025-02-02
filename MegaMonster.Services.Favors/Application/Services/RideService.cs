using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Application.Services;

public class RideService(IRideRepository rideRepository)
{
    public async Task<IEnumerable<Ride>> GetAllRides() => await rideRepository.GetAll();
    
    public async Task<Ride?> GetRideById(int id) => await rideRepository.GetRideById(id);

    public async Task<Ride?> GetRideByName(string name) => await rideRepository.GetRideByName(name);
    
    public async Task<ResultOperation> AddRide(Ride ride)
    {
        if (string.IsNullOrWhiteSpace(ride.Name)) 
            return ResultOperation.Fail("Ride name cannot be empty.");

        var existingCategory = await rideRepository.GetRideByName(ride.Name);
        if (existingCategory is not null)
            return ResultOperation.Fail($"Ride with name '{ride.Name}' already exists.");
        
        return await rideRepository.AddAsync(ride)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error adding ride.");
    }
    
    public async Task<ResultOperation> EditRide(string currentName, string newName, int categoryId, string status, double rating)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return ResultOperation.Fail("New ride name cannot be empty.");

        var currentRide = await rideRepository.GetRideByName(currentName);
        if (currentRide is null)
            return ResultOperation.Fail($"Ride '{currentName}' not found.");

        if (await rideRepository.GetRideByName(newName) is not null)
            return ResultOperation.Fail($"Ride with name '{newName}' already exists.");

        currentRide.Name = newName;
        currentRide.Rating = rating;
        currentRide.ClientStatus = status;
        currentRide.CategoryId = categoryId;
        return await rideRepository.EditAsync(currentRide)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error updating ride.");
    }
    
    public async Task<ResultOperation> DeleteRide(string rideName)
    {
        var ride = await rideRepository.GetRideByName(rideName);
        if (ride is null)
            return ResultOperation.Fail($"Ride '{rideName}' not found.");

        return await rideRepository.DeleteAsync(ride)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error deleting ride.");
    }
    
}