using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Core.Interfaces;

public interface IOrderDetailsRepository : IRepository<OrderDetails>
{
    public Task<IEnumerable<OrderDetails>> GetDetailsForOrder(List<int> orderIds);
}