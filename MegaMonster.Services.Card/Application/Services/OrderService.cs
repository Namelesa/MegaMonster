using MegaMonster.Services.Card.Application.ResultOperation;
using MegaMonster.Services.Card.Core.Interfaces;
using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Infrastructure;
using MegaMonster.Services.Card.Infrastructure.Redis;

namespace MegaMonster.Services.Card.Application.Services;

public class OrderService(IOrderRepository orderRepository, IRedisService redisService)
{
    public async Task<List<int>> GetOrdersIdByUserId(string userId)
    {
        var cacheKey = $"OrdersId_{userId}";
        var cachedOrders = await redisService.GetAsync<List<int>>(cacheKey);
        if (cachedOrders != null)
        {
            return cachedOrders;
        }

        var orders = await orderRepository.GetOrdersIdByUserId(userId);

        await redisService.SetAsync(cacheKey, orders, TimeSpan.FromMinutes(30));

        return orders;
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

            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult.Ok();
        }
        return OperationResult.Fail("Not found order with this id");
    }

    public async Task<OperationResult> AddOrder(Order order)
    {
        var result = await orderRepository.AddAsync(order);
        if (result)
        {
            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult.Ok();
        }
        return OperationResult.Fail("Error with adding order");
    }
    
    public async Task<Order?> GetAllOrder(int orderId)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
        return order;
    }
    
    public async Task<OperationResult> UpdateOrder(Order order)
    {
        var result = await orderRepository.EditAsync(order);
        if (result)
        {
            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult.Ok();
        }
        return OperationResult.Fail("Cannot update this order");
    }
}