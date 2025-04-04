using MegaMonster.Services.Favors.Core.BaseModel;

namespace MegaMonster.Services.Favors.Core.Ride;

public interface IRideRepository : IRepository<Ride>
{
    Task<Ride?> GetRideById(int id);
    Task<Ride?> GetRideByName(string name);
}