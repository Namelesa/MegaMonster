using MegaMonster.Services.Favors.Core.Interfaces;

namespace MegaMonster.Services.Favors.Core.Ride;

public interface IRideRepository : IRepository<Ride>
{
    Task<Ride?> GetRideById(int id);
    Task<Ride?> GetRideByName(string name);
}