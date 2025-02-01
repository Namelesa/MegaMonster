using MegaMonster.Services.Card.Application.ResultOperation;
using MegaMonster.Services.Card.Core.Interfaces;
using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Application.Services;

public class OrderService(IOrderRepository orderRepository)
{
    public async Task<List<int>> GetOrdersIdByUserId(string userId)
    {
        return await orderRepository.GetOrdersIdByUserId(userId);
    }

    public async Task<List<Order>> GetOrdersByUserId(string userId)
    {
        return await orderRepository.GetOrdersUserId(userId);
    }

    public async Task<OperationResult> DeleteById(int id)
    {
        var order = await orderRepository.GetOrder(id);
        if (order != null)
        {
            await orderRepository.DeleteAsync(order);
            return OperationResult.Ok();
        }
        return OperationResult.Fail("Not found order with this id");
    }

    public async Task<OperationResult> AddOrder(Order order)
    {
        var result = await orderRepository.AddAsync(order);
        return result ? OperationResult.Ok() : OperationResult.Fail("Error with adding order");
    }
    
    public async Task<Order?> GetAllOrder(int orderId)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
        return order;
    }
    
    public async Task<OperationResult> UpdateOrder(Order order)
    {
        var result = await orderRepository.EditAsync(order);
        return result ? OperationResult.Ok() : OperationResult.Fail("Can not update this order");
    }
}