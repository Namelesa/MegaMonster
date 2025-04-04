using MassTransit;
using MegaMonster.MessagingModels.Bill;
using MegaMonster.MessagingModels.Payment.Card;
using MegaMonster.MessagingModels.Payment.Cash;
using MegaMonster.MessagingModels.User.GetInfo;
using MegaMonster.Services.Card.Core;
using MegaMonster.Services.Card.Core.Order;
using MegaMonster.Services.Card.Infrastructure.Redis;

namespace MegaMonster.Services.Card.Application.Order;

public class OrderService(IOrderRepository orderRepository, 
    IRedisService redisService, 
    IPublishEndpoint publishEndpoint,
    IRequestClient<UserEmailRequest> userRequestClient)
{
    public async Task<List<Core.Order.Order>> GetOrdersByUserId(Guid userId)
    {
        return await orderRepository.GetOrdersUserId(userId);
    }
    
    public async Task<List<Core.Order.Order>> GetOrderHistoryByUserId(Guid userId)
    {
        var orders = await orderRepository.GetOrdersUserId(userId);
        var history = orders.Where(u => u.Status == Wc.PayedStatus).ToList();
        return history;
    }

    public async Task<OperationResult<string>> DeleteById(Guid id)
    {
        var order = await orderRepository.GetOrder(id);
        if (order != null)
        {
            await orderRepository.DeleteAsync(order);

            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult<string>.Ok("Deleted order!");
        }
        return OperationResult<string>.Fail("Not found order with this id");
    }

    public async Task<OperationResult<string>> AddOrder(Core.Order.Order order)
    {
        var result = await orderRepository.AddAsync(order);
        if (!result) return OperationResult<string>.Fail("Error with adding order");
        var cacheKey = $"Orders_{order.UserId}";
        await redisService.RemoveAsync(cacheKey);

        return OperationResult<string>.Ok("Add successful");
    }
    
    public async Task<Core.Order.Order?> GetAllOrder(Guid orderId)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
        return order;
    }
    
    public async Task<OperationResult<string>> UpdateOrder(Core.Order.Order order)
    {
        var result = await orderRepository.EditAsync(order);
        if (!result) return OperationResult<string>.Fail("Cannot update this order");
        var cacheKey = $"Orders_{order.UserId}";
        await redisService.RemoveAsync(cacheKey);

        return OperationResult<string>.Ok("Successful updated");
    }
    
    public async Task<OperationResult<Core.Order.Order>> UpdateOrderStatus(Guid orderId, string? bill, string status)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
        
        switch (order)
        {
            case null:
                return OperationResult<Core.Order.Order>.Fail($"Order with ID {orderId} not found.");
            case { Status: Wc.CreatedStatus, Bill: null } when status == Wc.PayedStatus:
                Console.WriteLine("Payed");
                order.Status = Wc.PayedStatus;
                break;
            case { Status: Wc.PayedStatus, Bill: not null } when status == Wc.CanceledStatus:
                Console.WriteLine("Canceled");
                order.Status = Wc.CanceledStatus;
                break;
        }
        
        order.Bill = bill;
        var result = await orderRepository.EditAsync(order);

        if (!result) return OperationResult<Core.Order.Order>.Fail($"Failed to update order {orderId}");
        
        var response = await userRequestClient.GetResponse<UserEmailResponse>(new UserEmailRequest{Id = order.UserId});

        var ticketsId = await orderRepository.GetTicketsIdByUserId(order.UserId); 
        
        var notifyUserBill = new InfoBillModel(order.UserName, order.PaymentType, order.Status, order.Sum, orderId, response.Message.Email)
        {
            TicketsIds = ticketsId
        };
        await publishEndpoint.Publish(notifyUserBill);
        return OperationResult<Core.Order.Order>.Ok(order);
    }
    
    public async Task<OperationResult<string>> CardCheckout(Guid userId)
    {
        var orders = await orderRepository.GetOrdersUserId(userId);
        
        var filteredOrdersCard = orders
            .Where(order => order is { Status: Wc.CreatedStatus, PaymentType: Wc.PaymentTypeCard })
            .ToList();
        
        var filteredOrdersCash = orders
            .Where(order => order is { Status: Wc.CreatedStatus, PaymentType: Wc.PaymentTypeCash })
            .ToList();
        
        var paymentInfoList = filteredOrdersCard.Select(order => new InfoPaymentModel(orderId: order.Id, count: order.OrderDetails.Count, userName: order.UserName, sum: order.Sum)).ToList();

        var paymentInfo = new InfoPaymentList()
        {
            Payments = paymentInfoList
        };
        Console.WriteLine($"Test = {paymentInfo.Payments.Count}");
        if (paymentInfo.Payments.Count > 0)
        {
            await publishEndpoint.Publish(paymentInfo);
        }
        
        foreach (var order in filteredOrdersCash)
        {
            var response = await userRequestClient.GetResponse<UserEmailResponse>(new UserEmailRequest{Id = order.UserId});
            var ticketsId = await orderRepository.GetTicketsIdByUserId(order.UserId); 
            
            var notifyUserBill = new InfoBillModel(order.UserName, order.PaymentType, order.Status, order.Sum, order.Id, response.Message.Email)
            {
                TicketsIds = ticketsId
            };
            await publishEndpoint.Publish(notifyUserBill);
        }
        
        var paymentInfoCashList = filteredOrdersCash.Select(order => new InfoPaymentCash(orderId: order.Id, count: order.OrderDetails.Count, userName: order.UserName, sum: order.Sum)).ToList();

        var paymentInfoCash = new InfoPaymentListCash()
        {
            Payments = paymentInfoCashList
        };
        if (paymentInfoCash.Payments.Count <= 0) return OperationResult<string>.Ok("");
        Console.WriteLine($"Processing cash payments: {paymentInfoCash.Payments.Count}");
        
        await publishEndpoint.Publish(paymentInfoCash);
        
        return OperationResult<string>.Ok("");
    }
}