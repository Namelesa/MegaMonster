using MegaMonster.Services.Favors.Core.Ride;
using MegaMonster.Services.Favors.Infrastructure.Redis;

namespace MegaMonster.Services.Favors.Application.Ride;

public class RideService(IRideRepository rideRepository, IRedisService redisService)
{
    private const string RideCacheKey = "All_Rides";

    public async Task<IEnumerable<Core.Ride.Ride>> GetAllRides() => 
        await GetOrSetCache(RideCacheKey, () => rideRepository.GetAll()!);
    
    public async Task<Core.Ride.Ride?> GetRideById(int id) => 
        await GetOrSetCache($"Ride_{id}", () => rideRepository.GetRideById(id));

    public async Task<Core.Ride.Ride?> GetRideByName(string name) => 
        await GetOrSetCache($"Ride_{name}", () => rideRepository.GetRideByName(name));
    
    public async Task<ResultOperation> AddRide(Core.Ride.Ride ride)
    {
        if (string.IsNullOrWhiteSpace(ride.Name)) 
            return ResultOperation.Fail("Ride name cannot be empty.");

        if (await rideRepository.GetRideByName(ride.Name) is not null)
            return ResultOperation.Fail($"Ride with name '{ride.Name}' already exists.");
        
        var result = await rideRepository.AddAsync(ride);
        if (!result) return ResultOperation.Fail("Error adding ride.");
        
        await ProcessChange(ride.Name);
        return ResultOperation.Ok();
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

        var result = await rideRepository.EditAsync(currentRide);
        if (!result) return ResultOperation.Fail("Error updating ride.");
        
        await ProcessChange(currentName, newName);
        return ResultOperation.Ok();
    }
    
    public async Task<ResultOperation> DeleteRide(string rideName)
    {
        var ride = await rideRepository.GetRideByName(rideName);
        if (ride is null)
            return ResultOperation.Fail($"Ride '{rideName}' not found.");

        var result = await rideRepository.DeleteAsync(ride);
        if (!result) return ResultOperation.Fail("Error deleting ride.");
        
        await ProcessChange(rideName);
        return ResultOperation.Ok();
    }
    
    private async Task<T?> GetOrSetCache<T>(string key, Func<Task<T?>> getData, TimeSpan? expiration = null)
    {
        var cachedData = await redisService.GetAsync<T>(key);
        if (cachedData is not null) return cachedData;
        
        var data = await getData();
        if (data is not null)
            await redisService.SetAsync(key, data, expiration ?? TimeSpan.FromMinutes(60));
        
        return data;
    }
    
    private async Task ProcessChange(params string[] rideNames)
    {
        await redisService.RemoveAsync(RideCacheKey);
        foreach (var rideName in rideNames)
        {
            await redisService.RemoveAsync($"Ride_{rideName}");
        }
    }
}
