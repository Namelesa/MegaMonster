using MegaMonster.Services.Favors.Core.Ride;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Ride;

public class RideRepository(AppDbContext db) : IRideRepository
{
    public async Task<IEnumerable<Core.Ride.Ride>> GetAll() => await db.Rides.AsNoTracking<Core.Ride.Ride>().ToListAsync();
    
    public async Task<Core.Ride.Ride?> GetRideById(int id) => 
        await db.Rides.AsNoTracking<Core.Ride.Ride>().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Core.Ride.Ride?> GetRideByName(string name) => 
        await db.Rides.AsNoTracking<Core.Ride.Ride>().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<bool> AddAsync(Core.Ride.Ride ride)
    {
        try
        {
            await db.Rides.AddAsync(ride);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding ride: {e.Message}");
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.Ride.Ride ride)
    {
        try
        {
            db.Rides.Update(ride);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating ride: {e.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Core.Ride.Ride ride)
    {
        try
        {
            db.Rides.Remove(ride);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting ride: {e.Message}");
            return false;
        }
    }
}