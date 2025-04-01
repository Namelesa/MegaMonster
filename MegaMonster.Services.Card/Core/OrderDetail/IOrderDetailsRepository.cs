using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Core.OrderDetail;

public interface IOrderDetailsRepository : IRepository<OrderDetails>
{
    public Task<IEnumerable<OrderDetails>> GetDetailsForOrder(List<Guid> orderIds);
}