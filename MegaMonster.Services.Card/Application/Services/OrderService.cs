using MassTransit;
using MegaMonster.Services.Card.Application.ResultOperation;
using MegaMonster.Services.Card.Core;
using MegaMonster.Services.Card.Core.Interfaces;
using MegaMonster.Services.Card.Core.Models;
using MegaMonster.Services.Card.Infrastructure.Redis;
using MegaMonster.MessagingModels.InfoPayment;
using MegaMonster.MessagingModels.InfoPayment.CashPaymentModels;
using MegaMonster.MessagingModels.UserInformation.UserEmail;

namespace MegaMonster.Services.Card.Application.Services;

public class OrderService(IOrderRepository orderRepository, 
    IRedisService redisService, 
    IPublishEndpoint publishEndpoint,
    IRequestClient<UserEmailRequest> userRequestClient)
{
    public async Task<List<Order>> GetOrdersByUserId(Guid userId)
    {
        return await orderRepository.GetOrdersUserId(userId);
    }
    
    public async Task<List<Order>> GetOrderHistoryByUserId(Guid userId)
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

    public async Task<OperationResult<string>> AddOrder(Order order)
    {
        var result = await orderRepository.AddAsync(order);
        if (result)
        {
            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult<string>.Ok("Add successful");
        }
        return OperationResult<string>.Fail("Error with adding order");
    }
    
    public async Task<Order?> GetAllOrder(Guid orderId)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
        return order;
    }
    
    public async Task<OperationResult<string>> UpdateOrder(Order order)
    {
        var result = await orderRepository.EditAsync(order);
        if (result)
        {
            var cacheKey = $"Orders_{order.UserId}";
            await redisService.RemoveAsync(cacheKey);

            return OperationResult<string>.Ok("Successful updated");
        }
        return OperationResult<string>.Fail("Cannot update this order");
    }
    
    public async Task<OperationResult<Order>> UpdateOrderStatus(Guid orderId, string bill)
    {
        var order = await orderRepository.GetAllOrderInfo(orderId);
    
        if (order == null)
        {
            return OperationResult<Order>.Fail($"Order with ID {orderId} not found.");
        }
        
        if (order.Status == Wc.PayedStatus)
        {
            return OperationResult<Order>.Ok(order);
        }

        order.Status = Wc.PayedStatus;
        order.Bill = bill;
        var result = await orderRepository.EditAsync(order);

        if (!result) return OperationResult<Order>.Fail($"Failed to update order {orderId}");
        
        var response = await userRequestClient.GetResponse<UserEmailResponse>(new UserEmailRequest{Id = order.UserId});

        var ticketsId = await orderRepository.GetTicketsIdByUserId(order.UserId); 
        
        var notifyUserBill = new InfoBillModel(order.UserName, order.PaymentType, order.Status, order.Sum, orderId, response.Message.Email)
        {
            TicketsIds = ticketsId
        };
        await publishEndpoint.Publish(notifyUserBill);
        return OperationResult<Order>.Ok(order);
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
        
        var paymentInfoList = new List<InfoPaymentModel>();

        foreach (var order in filteredOrdersCard)
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
        
        var paymentInfoCashList = new List<InfoPaymentCash>();

        foreach (var order in filteredOrdersCash)
        {
            var infoForPayment = new InfoPaymentCash(
                orderId: order.Id, 
                count: order.OrderDetails.Count,
                userName: order.UserName,
                sum: order.Sum
            );

            paymentInfoCashList.Add(infoForPayment);
        }

        var paymentInfoCash = new InfoPaymentListCash()
        {
            Payments = paymentInfoCashList
        };
        if (paymentInfoCash.Payments.Count > 0)
        {
            Console.WriteLine($"Processing cash payments: {paymentInfoCash.Payments.Count}");
            await publishEndpoint.Publish(paymentInfoCash);
        }

        
        return OperationResult<string>.Ok("");
    }
}