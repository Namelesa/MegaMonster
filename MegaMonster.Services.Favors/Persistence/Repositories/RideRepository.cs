using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Repositories;

public class RideRepository(AppDbContext db) : IRideRepository
{
    public async Task<IEnumerable<Ride>> GetAll() => await db.Rides.AsNoTracking().ToListAsync();
    
    public async Task<Ride?> GetRideById(int id) => 
        await db.Rides.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Ride?> GetRideByName(string name) => 
        await db.Rides.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<bool> AddAsync(Ride ride)
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

    public async Task<bool> EditAsync(Ride ride)
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

    public async Task<bool> DeleteAsync(Ride ride)
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