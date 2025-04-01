using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Core.OrderDetail;
using MegaMonster.Services.Card.Infrastructure.Redis;

namespace MegaMonster.Services.Card.Application.Order;

public class OrderDetailsService(IOrderDetailsRepository orderDetailsRepository, IRedisService redisService)
{
    public async Task<IEnumerable<OrderDetails>> GetOrderDetails(List<Guid> orderIds)
    {
        var cacheKey = $"OrderDetails_{string.Join("_", orderIds)}";
        var cachedDetails = await redisService.GetAsync<IEnumerable<OrderDetails>>(cacheKey);
        if (cachedDetails != null)
        {
            return cachedDetails;
        }

        var orderDetails = await orderDetailsRepository.GetDetailsForOrder(orderIds);
        var orderDetailsEnumerable = orderDetails as OrderDetails[] ?? orderDetails.ToArray();
        await redisService.SetAsync(cacheKey, orderDetailsEnumerable, TimeSpan.FromMinutes(30));

        return orderDetailsEnumerable;
    }
}