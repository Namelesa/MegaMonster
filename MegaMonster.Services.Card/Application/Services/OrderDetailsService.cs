using MegaMonster.Services.Card.Application.ResultOperation;
using MegaMonster.Services.Card.Core.Interfaces;
using MegaMonster.Services.Card.Core.Models;

namespace MegaMonster.Services.Card.Application.Services;

public class OrderDetailsService(IOrderDetailsRepository orderDetailsRepository)
{
    public async Task<IEnumerable<OrderDetails>> GetOrderDetails(List<int> orderIds)
    {
        return await orderDetailsRepository.GetDetailsForOrder(orderIds);
    }
}