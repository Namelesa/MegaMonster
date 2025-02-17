using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    public Task<List<int>> GetOrdersIdByUserId(Guid userId);
    public Task<List<Order>> GetOrdersUserId(Guid userId);
    public Task<Order?> GetOrder(int id);
    public Task<Order?> GetAllOrderInfo(int orderId);
}