using MegaMonster.Services.Payment.Application.Payment;
using MegaMonster.Services.Payment.Core.Payment;
using Moq;

namespace MegaMonster.Services.Payment.Tests.UnitTests
{
    public class PaymentServiceRepositoryTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly PaymentServiceRepository _paymentServiceRepository;

        public PaymentServiceRepositoryTests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _paymentServiceRepository = new PaymentServiceRepository(_mockPaymentRepository.Object);
        }

        [Fact]
        public async Task GetPaymentByOrderId_WhenPaymentExists_ReturnsSuccessResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var payment = new Payments
            {
                OrderId = orderId,
                Status = "created",
                UserName = "testUser",
                Count = 1,
                Sum = 100,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockPaymentRepository.Setup(repo => repo.GetPaymentAsyncByOrderId(orderId))
                .ReturnsAsync(payment);

            // Act
            var result = await _paymentServiceRepository.GetPaymentByOrderId(orderId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(payment, result.Data);
        }

        [Fact]
        public async Task GetPaymentByOrderId_WhenPaymentDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockPaymentRepository.Setup(repo => repo.GetPaymentAsyncByOrderId(orderId))
                .ReturnsAsync((Payments)null);

            // Act
            var result = await _paymentServiceRepository.GetPaymentByOrderId(orderId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Not found payment for this order", result.Message);
        }

        [Fact]
        public async Task AddPayment_WhenSuccessful_ReturnsSuccessResult()
        {
            // Arrange
            var payment = new Payments
            {
                OrderId = Guid.NewGuid(),
                Status = "created",
                UserName = "testUser",
                Count = 1,
                Sum = 100,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockPaymentRepository.Setup(repo => repo.AddPaymentAsync(payment))
                .ReturnsAsync(true);

            // Act
            var result = await _paymentServiceRepository.AddPayment(payment);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Add new payment", result.Data);
        }

        [Fact]
        public async Task AddPayment_WhenFailed_ReturnsFailResult()
        {
            // Arrange
            var payment = new Payments
            {
                OrderId = Guid.NewGuid(),
                Status = "created",
                UserName = "testUser",
                Count = 1,
                Sum = 100,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockPaymentRepository.Setup(repo => repo.AddPaymentAsync(payment))
                .ReturnsAsync(false);

            // Act
            var result = await _paymentServiceRepository.AddPayment(payment);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Can not add this payment", result.Message);
        }

        [Fact]
        public async Task UpdatePayment_WhenSuccessful_ReturnsSuccessResult()
        {
            // Arrange
            var payment = new Payments
            {
                OrderId = Guid.NewGuid(),
                Status = "success",
                UserName = "testUser",
                Count = 1,
                Sum = 100,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockPaymentRepository.Setup(repo => repo.UpdatePaymentAsync(payment))
                .ReturnsAsync(true);

            // Act
            var result = await _paymentServiceRepository.UpdatePayment(payment);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("update payment", result.Data);
        }

        [Fact]
        public async Task UpdatePayment_WhenFailed_ReturnsFailResult()
        {
            // Arrange
            var payment = new Payments
            {
                OrderId = Guid.NewGuid(),
                Status = "success",
                UserName = "testUser",
                Count = 1,
                Sum = 100,
                CreatedAt = DateTime.UtcNow
            };
            
            _mockPaymentRepository.Setup(repo => repo.UpdatePaymentAsync(payment))
                .ReturnsAsync(false);

            // Act
            var result = await _paymentServiceRepository.UpdatePayment(payment);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Can not update this payment", result.Message);
        }
    }
}