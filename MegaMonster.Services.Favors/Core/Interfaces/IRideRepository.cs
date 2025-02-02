using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface IRideRepository : IRepository<Ride>
{
    Task<Ride?> GetRideById(int id);
    Task<Ride?> GetRideByName(string name);
}