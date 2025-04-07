using MegaMonster.Services.Card.Application.Order;
using MegaMonster.Services.Card.Core.OrderDetail;
using MegaMonster.Services.Card.Infrastructure.Redis;
using Moq;

namespace MegaMonster.Services.Card.Tests.UnitTests
{
    public class OrderDetailsServiceTests
    {
        private readonly Mock<IOrderDetailsRepository> _mockOrderDetailsRepository;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly OrderDetailsService _orderDetailsService;

        public OrderDetailsServiceTests()
        {
            _mockOrderDetailsRepository = new Mock<IOrderDetailsRepository>();
            _mockRedisService = new Mock<IRedisService>();
            _orderDetailsService = new OrderDetailsService(
                _mockOrderDetailsRepository.Object, 
                _mockRedisService.Object);
        }

        [Fact]
        public async Task GetOrderDetails_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var orderIds = new List<Guid> { new(), new() };
            var cacheKey = $"OrderDetails_{string.Join("_", orderIds)}";
            var cachedDetails = new List<OrderDetails>
            {
                new() { OrderId = new Guid() },
                new() { OrderId = new Guid() }
            };

            _mockRedisService
                .Setup(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey))
                .ReturnsAsync(cachedDetails);

            // Act
            var result = await _orderDetailsService.GetOrderDetails(orderIds);

            // Assert
            Assert.Equal(cachedDetails, result);
            _mockRedisService.Verify(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey), Times.Once);
            _mockOrderDetailsRepository.Verify(x => x.GetDetailsForOrder(It.IsAny<List<Guid>>()), Times.Never);
        }

        [Fact]
        public async Task GetOrderDetails_WhenCacheDoesNotExist_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var orderIds = new List<Guid> { new(), new() };
            var cacheKey = $"OrderDetails_{string.Join("_", orderIds)}";
            var repositoryDetails = new List<OrderDetails>
            {
                new () { OrderId = new Guid() },
                new () { OrderId = new Guid() }
            };

            _mockRedisService
                .Setup(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey))
                .ReturnsAsync((IEnumerable<OrderDetails>)null);

            _mockOrderDetailsRepository
                .Setup(x => x.GetDetailsForOrder(orderIds))
                .ReturnsAsync(repositoryDetails);

            // Act
            var result = await _orderDetailsService.GetOrderDetails(orderIds);

            // Assert
            Assert.Equal(repositoryDetails, result);
            _mockRedisService.Verify(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey), Times.Once);
            _mockOrderDetailsRepository.Verify(x => x.GetDetailsForOrder(orderIds), Times.Once);
            _mockRedisService.Verify(x => x.SetAsync(
                cacheKey, 
                It.Is<IEnumerable<OrderDetails>>(r => r.SequenceEqual(repositoryDetails)),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(30))), 
                Times.Once);
        }

        [Fact]
        public async Task GetOrderDetails_WhenRepositoryReturnsEmpty_ShouldCacheEmptyResult()
        {
            // Arrange
            var orderIds = new List<Guid> { new(), new() };
            var cacheKey = $"OrderDetails_{string.Join("_", orderIds)}";
            var emptyResult = new List<OrderDetails>();

            _mockRedisService
                .Setup(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey))
                .ReturnsAsync((IEnumerable<OrderDetails>)null);

            _mockOrderDetailsRepository
                .Setup(x => x.GetDetailsForOrder(orderIds))
                .ReturnsAsync(emptyResult);

            // Act
            var result = await _orderDetailsService.GetOrderDetails(orderIds);

            // Assert
            Assert.Empty(result);
            _mockRedisService.Verify(x => x.SetAsync(
                cacheKey,
                It.Is<IEnumerable<OrderDetails>>(d => !d.Any()),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(30))),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderDetails_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var orderIds = new List<Guid> { new(), new() };
            var cacheKey = $"OrderDetails_{string.Join("_", orderIds)}";
            var expectedException = new Exception("Database connection error");

            _mockRedisService
                .Setup(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey))
                .ReturnsAsync((IEnumerable<OrderDetails>)null);

            _mockOrderDetailsRepository
                .Setup(x => x.GetDetailsForOrder(orderIds))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _orderDetailsService.GetOrderDetails(orderIds));
            
            Assert.Equal(expectedException.Message, exception.Message);
            _mockRedisService.Verify(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<OrderDetails>>(),
                It.IsAny<TimeSpan>()),
                Times.Never);
        }

        [Fact]
        public async Task GetOrderDetails_WithEmptyOrderIdsList_ShouldReturnEmptyResult()
        {
            // Arrange
            var emptyOrderIds = new List<Guid>();
            var cacheKey = $"OrderDetails_{string.Join("_", emptyOrderIds)}";

            _mockRedisService
                .Setup(x => x.GetAsync<IEnumerable<OrderDetails>>(cacheKey))
                .ReturnsAsync((IEnumerable<OrderDetails>)null);

            _mockOrderDetailsRepository
                .Setup(x => x.GetDetailsForOrder(emptyOrderIds))
                .ReturnsAsync(new List<OrderDetails>());

            // Act
            var result = await _orderDetailsService.GetOrderDetails(emptyOrderIds);

            // Assert
            Assert.Empty(result);
        }
    }
}