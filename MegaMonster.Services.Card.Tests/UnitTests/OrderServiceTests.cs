using MassTransit;
using MegaMonster.MessagingModels.Bill;
using MegaMonster.MessagingModels.Payment.Card;
using MegaMonster.MessagingModels.Payment.Cash;
using MegaMonster.MessagingModels.User.GetInfo;
using MegaMonster.Services.Card.Application.Order;
using MegaMonster.Services.Card.Core;
using MegaMonster.Services.Card.Core.Order;
using MegaMonster.Services.Card.Core.OrderDetail;
using MegaMonster.Services.Card.Infrastructure.Redis;
using Moq;

namespace MegaMonster.Services.Card.Tests.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly Mock<IRequestClient<UserEmailRequest>> _mockUserRequestClient;
        private readonly OrderService _orderService;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testOrderId = Guid.NewGuid();

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockRedisService = new Mock<IRedisService>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _mockUserRequestClient = new Mock<IRequestClient<UserEmailRequest>>();

            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockRedisService.Object,
                _mockPublishEndpoint.Object,
                _mockUserRequestClient.Object);
        }

        #region GetOrdersByUserId Tests

        [Fact]
        public async Task GetOrdersByUserId_ShouldReturnOrders_WhenUserIdExists()
        {
            // Arrange
            var expectedOrders = new List<Order>
            {
                new() { Id = Guid.NewGuid(), UserId = _testUserId },
                new() { Id = Guid.NewGuid(), UserId = _testUserId }
            };

            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(expectedOrders);

            // Act
            var result = await _orderService.GetOrdersByUserId(_testUserId);

            // Assert
            Assert.Equal(expectedOrders.Count, result.Count);
            Assert.Equal(expectedOrders, result);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
        }

        [Fact]
        public async Task GetOrdersByUserId_ShouldReturnEmptyList_WhenUserHasNoOrders()
        {
            // Arrange
            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync([]);

            // Act
            var result = await _orderService.GetOrdersByUserId(_testUserId);

            // Assert
            Assert.Empty(result);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
        }

        #endregion

        #region GetOrderHistoryByUserId Tests

        [Fact]
        public async Task GetOrderHistoryByUserId_ShouldReturnOnlyPayedOrders()
        {
            // Arrange
            var allOrders = new List<Order>
            {
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.PayedStatus },
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.CreatedStatus },
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.PayedStatus },
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.CanceledStatus }
            };

            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(allOrders);

            // Act
            var result = await _orderService.GetOrderHistoryByUserId(_testUserId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, order => Assert.Equal(Wc.PayedStatus, order.Status));
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
        }

        [Fact]
        public async Task GetOrderHistoryByUserId_ShouldReturnEmptyList_WhenNoPayedOrders()
        {
            // Arrange
            var allOrders = new List<Order>
            {
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.CreatedStatus },
                new() { Id = Guid.NewGuid(), UserId = _testUserId, Status = Wc.CanceledStatus }
            };

            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(allOrders);

            // Act
            var result = await _orderService.GetOrderHistoryByUserId(_testUserId);

            // Assert
            Assert.Empty(result);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
        }

        #endregion

        #region DeleteById Tests

        [Fact]
        public async Task DeleteById_ShouldReturnSuccess_WhenOrderExists()
        {
            // Arrange
            var order = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.GetOrder(_testOrderId))
                .ReturnsAsync(order);
            
            _mockOrderRepository
                .Setup(repo => repo.DeleteAsync(order))
                .ReturnsAsync(true);
            
            _mockRedisService
                .Setup(redis => redis.RemoveAsync($"Orders_{_testUserId}"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.DeleteById(_testOrderId);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetOrder(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.DeleteAsync(order), Times.Once);
            _mockRedisService.Verify(redis => redis.RemoveAsync($"Orders_{_testUserId}"), Times.Once);
        }

        [Fact]
        public async Task DeleteById_ShouldReturnFailure_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository
                .Setup(repo => repo.GetOrder(_testOrderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.DeleteById(_testOrderId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Not found order with this id", result.Message);
            _mockOrderRepository.Verify(repo => repo.GetOrder(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Order>()), Times.Never);
            _mockRedisService.Verify(redis => redis.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region AddOrder Tests

        [Fact]
        public async Task AddOrder_ShouldReturnSuccess_WhenAddingSucceeds()
        {
            // Arrange
            var order = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.AddAsync(order))
                .ReturnsAsync(true);
            
            _mockRedisService
                .Setup(redis => redis.RemoveAsync($"Orders_{_testUserId}"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.AddOrder(order);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.AddAsync(order), Times.Once);
            _mockRedisService.Verify(redis => redis.RemoveAsync($"Orders_{_testUserId}"), Times.Once);
        }

        [Fact]
        public async Task AddOrder_ShouldReturnFailure_WhenAddingFails()
        {
            // Arrange
            var order = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.AddAsync(order))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.AddOrder(order);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Error with adding order", result.Message);
            _mockOrderRepository.Verify(repo => repo.AddAsync(order), Times.Once);
            _mockRedisService.Verify(redis => redis.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region GetAllOrder Tests

        [Fact]
        public async Task GetAllOrder_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var expectedOrder = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.GetAllOrderInfo(_testOrderId))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _orderService.GetAllOrder(_testOrderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrder, result);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
        }

        [Fact]
        public async Task GetAllOrder_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository
                .Setup(repo => repo.GetAllOrderInfo(_testOrderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetAllOrder(_testOrderId);

            // Assert
            Assert.Null(result);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
        }

        #endregion

        #region UpdateOrder Tests

        [Fact]
        public async Task UpdateOrder_ShouldReturnSuccess_WhenUpdateSucceeds()
        {
            // Arrange
            var order = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.EditAsync(order))
                .ReturnsAsync(true);
            
            _mockRedisService
                .Setup(redis => redis.RemoveAsync($"Orders_{_testUserId}"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.UpdateOrder(order);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.EditAsync(order), Times.Once);
            _mockRedisService.Verify(redis => redis.RemoveAsync($"Orders_{_testUserId}"), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_ShouldReturnFailure_WhenUpdateFails()
        {
            // Arrange
            var order = new Order { Id = _testOrderId, UserId = _testUserId };
            
            _mockOrderRepository
                .Setup(repo => repo.EditAsync(order))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.UpdateOrder(order);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot update this order", result.Message);
            _mockOrderRepository.Verify(repo => repo.EditAsync(order), Times.Once);
            _mockRedisService.Verify(redis => redis.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region UpdateOrderStatus Tests

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnFailure_WhenOrderNotFound()
        {
            // Arrange
            _mockOrderRepository
                .Setup(repo => repo
                .GetAllOrderInfo(_testOrderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.UpdateOrderStatus(_testOrderId, null, Wc.PayedStatus);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Order with ID {_testOrderId} not found.", result.Message);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.EditAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldUpdateToPayedStatus_WhenConditionsAreMet()
        {
            // Arrange
            var order = new Order 
            { 
                Id = _testOrderId, 
                UserId = _testUserId, 
                Status = Wc.CreatedStatus,
                Bill = null,
                UserName = "TestUser",
                Sum = 100,
                PaymentType = "Card",
                OrderDetails = new List<OrderDetails>()
            };
            
            var userEmailResponse = new UserEmailResponse { Email = "user@example.com" };
            var responseMessage = new Mock<Response<UserEmailResponse>>();
            responseMessage.Setup(r => r.Message).Returns(userEmailResponse);
            
            _mockOrderRepository
                .Setup(repo => repo.GetAllOrderInfo(_testOrderId))
                .ReturnsAsync(order);
            
            _mockOrderRepository
                .Setup(repo => repo.EditAsync(It.IsAny<Order>()))
                .ReturnsAsync(true);

            _mockOrderRepository
                .Setup(repo => repo.GetTicketsIdByUserId(_testUserId))
                .ReturnsAsync([]);
            
            _mockUserRequestClient
                .Setup(client => client.GetResponse<UserEmailResponse>(
                    It.IsAny<UserEmailRequest>(), 
                    It.IsAny<CancellationToken>(), 
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(responseMessage.Object);

            // Act
            var result = await _orderService.UpdateOrderStatus(_testOrderId, null, Wc.PayedStatus);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.EditAsync(It.IsAny<Order>()), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldUpdateToCanceledStatus_WhenConditionsAreMet()
        {
            // Arrange
            var order = new Order 
            { 
                Id = _testOrderId, 
                UserId = _testUserId, 
                Status = Wc.PayedStatus,
                Bill = "Bill123",
                UserName = "TestUser",
                Sum = 100,
                PaymentType = "Card",
                OrderDetails = new List<OrderDetails>()
            };
            
            var userEmailResponse = new UserEmailResponse { Email = "user@example.com" };
            var responseMessage = new Mock<Response<UserEmailResponse>>();
            responseMessage.Setup(r => r.Message).Returns(userEmailResponse);
            
            _mockOrderRepository
                .Setup(repo => repo.GetAllOrderInfo(_testOrderId))
                .ReturnsAsync(order);
            
            _mockOrderRepository
                .Setup(repo => repo.EditAsync(It.IsAny<Order>()))
                .ReturnsAsync(true);

            _mockOrderRepository
                .Setup(repo => repo.GetTicketsIdByUserId(_testUserId))
                .ReturnsAsync([]);
            
            _mockUserRequestClient
                .Setup(client => client.GetResponse<UserEmailResponse>(
                    It.IsAny<UserEmailRequest>(), 
                    It.IsAny<CancellationToken>(), 
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(responseMessage.Object);

            // Act
            var result = await _orderService.UpdateOrderStatus(_testOrderId, "Bill123", Wc.CanceledStatus);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.EditAsync(It.IsAny<Order>()), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnFailure_WhenEditFails()
        {
            // Arrange
            var order = new Order 
            { 
                Id = _testOrderId, 
                UserId = _testUserId, 
                Status = Wc.CreatedStatus,
                Bill = null
            };
            
            _mockOrderRepository
                .Setup(repo => repo.GetAllOrderInfo(_testOrderId))
                .ReturnsAsync(order);
            
            _mockOrderRepository
                .Setup(repo => repo.EditAsync(It.IsAny<Order>()))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.UpdateOrderStatus(_testOrderId, null, Wc.PayedStatus);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Failed to update order {_testOrderId}", result.Message);
            _mockOrderRepository.Verify(repo => repo.GetAllOrderInfo(_testOrderId), Times.Once);
            _mockOrderRepository.Verify(repo => repo.EditAsync(It.IsAny<Order>()), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region CardCheckout Tests

        [Fact]
        public async Task CardCheckout_ShouldProcessCardAndCashPayments()
        {
            // Arrange
            var cardOrder = new Order 
            { 
                Id = Guid.NewGuid(), 
                UserId = _testUserId, 
                Status = Wc.CreatedStatus,
                PaymentType = Wc.PaymentTypeCard,
                UserName = "TestUser",
                Sum = 100,
                OrderDetails = new List<OrderDetails>()
            };
            
            var cashOrder = new Order 
            { 
                Id = Guid.NewGuid(), 
                UserId = _testUserId, 
                Status = Wc.CreatedStatus,
                PaymentType = Wc.PaymentTypeCash,
                UserName = "TestUser",
                Sum = 50,
                OrderDetails = new List<OrderDetails>()
            };
            
            var orders = new List<Order> { cardOrder, cashOrder };
            
            var userEmailResponse = new UserEmailResponse { Email = "user@example.com" };
            var responseMessage = new Mock<Response<UserEmailResponse>>();
            responseMessage.Setup(r => r.Message).Returns(userEmailResponse);

            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(orders);
            
            _mockOrderRepository
                .Setup(repo => repo.GetTicketsIdByUserId(_testUserId))
                .ReturnsAsync([]);
            
            _mockUserRequestClient
                .Setup(client => client.GetResponse<UserEmailResponse>(
                    It.IsAny<UserEmailRequest>(), 
                    It.IsAny<CancellationToken>(), 
                    It.IsAny<RequestTimeout>()))
                .ReturnsAsync(responseMessage.Object);

            // Act
            var result = await _orderService.CardCheckout(_testUserId);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentList>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentListCash>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CardCheckout_ShouldNotPublishPayments_WhenNoOrdersFound()
        {
            // Arrange
            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _orderService.CardCheckout(_testUserId);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentList>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentListCash>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CardCheckout_ShouldOnlyProcessApplicableOrders()
        {
            // Arrange
            var completedOrder = new Order 
            { 
                Id = Guid.NewGuid(), 
                UserId = _testUserId, 
                Status = Wc.PayedStatus,
                PaymentType = Wc.PaymentTypeCard
            };
            
            var canceledOrder = new Order 
            { 
                Id = Guid.NewGuid(), 
                UserId = _testUserId, 
                Status = Wc.CanceledStatus,
                PaymentType = Wc.PaymentTypeCash
            };
            
            var orders = new List<Order> { completedOrder, canceledOrder };

            _mockOrderRepository
                .Setup(repo => repo.GetOrdersUserId(_testUserId))
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.CardCheckout(_testUserId);

            // Assert
            Assert.True(result.Success);
            _mockOrderRepository.Verify(repo => repo.GetOrdersUserId(_testUserId), Times.Once);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentList>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoBillModel>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockPublishEndpoint.Verify(pe => pe.Publish(It.IsAny<InfoPaymentListCash>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #endregion
    }
}