using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    public Task<List<Guid>> GetOrdersIdByUserId(Guid userId);
    public Task<List<Order>> GetOrdersUserId(Guid userId);
    public Task<Order?> GetOrder(Guid id);
    public Task<Order?> GetAllOrderInfo(Guid orderId);
    Task<List<int>> GetTicketsIdByUserId(Guid userId);
}