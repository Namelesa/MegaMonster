using MassTransit;
using MegaMonster.Services.Card.Application.ResultOperation;
using MegaMonster.Services.Card.Core;
using MegaMonster.Services.Card.Core.Interfaces;
using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Infrastructure.Redis;
using MessagingModels.InfoPayment;

namespace MegaMonster.Services.Card.Application.Services;

public class OrderService(IOrderRepository orderRepository, IRedisService redisService, IPublishEndpoint publishEndpoint)
{
    public async Task<List<int>> GetOrdersIdByUserId(Guid userId)
    {
        var cacheKey = $"OrdersId_{userId}";
        var cachedOrders = await redisService.GetAsync<List<int>>(cacheKey);
        if (cachedOrders != null && cachedOrders.Count != 0)
        {
            return cachedOrders;
        }

        var orders = await orderRepository.GetOrdersIdByUserId(userId);

        await redisService.SetAsync(cacheKey, orders, TimeSpan.FromMinutes(30));

        return orders;
    }

    public async Task<List<Order>> GetOrdersByUserId(Guid userId)
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
    
    public async Task<OperationResult> CardCheckout(Guid userId)
    {
        var orders = await orderRepository.GetOrdersUserId(userId);
        
        var filteredOrders = orders
            .Where(order => order.Status == Wc.CreatedStatus && order.PaymentType == Wc.PaymentTypeCard)
            .ToList();
        
        var paymentInfoList = new List<InfoPaymentModel>();

        foreach (var order in filteredOrders)
        {
            var infoForPayment = new InfoPaymentModel(
                orderId: order.Id, 
                count: order.OrderDetails.Count,
                userName: order.UserName,
                sum: order.Sum
            );

            paymentInfoList.Add(infoForPayment);
        }

        var paymentInfo = new InfoPaymentList()
        {
            Payments = paymentInfoList
        };
        await publishEndpoint.Publish(paymentInfo);
        return OperationResult.Ok();
    }
}