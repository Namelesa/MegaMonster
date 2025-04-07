using System.Text;
using MegaMonster.Services.Payment.Application.Payment;
using MegaMonster.Services.Payment.Core.Payment;
using MegaMonster.Services.Payment.Infrastructure.Payment;
using Moq;

namespace MegaMonster.Services.Payment.Tests.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        var paymentServiceRepository = new PaymentServiceRepository(_paymentRepositoryMock.Object);
        _paymentService = new PaymentService("publicKey", "privateKey", paymentServiceRepository);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldGeneratePaymentUrl_WhenValidInput()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userName = "testUser";
        var amount = 100.0;
        var count = 2;
        var action = "purchase";

        _paymentRepositoryMock
            .Setup(repo => repo.AddPaymentAsync(It.IsAny<Payments>()))
            .ReturnsAsync(true);

        // Act
        var result = await _paymentService.CreatePaymentAsync(orderId, userName, amount, count, action);

        // Assert
        Assert.StartsWith("https://www.liqpay.ua", result);
        _paymentRepositoryMock.Verify(repo => repo.AddPaymentAsync(It.IsAny<Payments>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnError_WhenCountOrAmountInvalid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userName = "testUser";
        var amount = -100.0; // Invalid amount
        var count = 0; // Invalid count
        var action = "purchase";

        // Act
        var result = await _paymentService.CreatePaymentAsync(orderId, userName, amount, count, action);

        // Assert
        Assert.Equal("Count and sum must be > 0", result);
    }
    
    [Fact]
    public async Task HandlePaymentResultAsync_ShouldReturnFailure_WhenSignatureMismatch()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"orderId\": \"" + orderId + "\"}"));
    
        var data = new Dictionary<string, string>
        {
            { "data", base64Data },
            { "signature", "invalidSignature" }
        };

        _paymentRepositoryMock
            .Setup(repo => repo.GetPaymentAsyncByOrderId(It.IsAny<Guid>()))
            .ReturnsAsync(new Payments());

        // Act
        var result = await _paymentService.HandlePaymentResultAsync(data);

        // Assert
        Assert.False(result.isSuccess);
        Assert.Null(result.transactionId);
    }
    
    [Fact]
    public async Task CancelPaymentAsync_ShouldReturnFalse_WhenPaymentNotFoundOrStatusIsNotSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = new Payments
        {
            OrderId = orderId,
            Status = PaymentSettings.IsCreated,
            UserName = "testUser",
            Count = 1,
            Sum = 100.0,
            CreatedAt = DateTime.UtcNow
        };

        _paymentRepositoryMock
            .Setup(repo => repo.GetPaymentAsyncByOrderId(orderId))
            .ReturnsAsync(payment);

        // Act
        var result = await _paymentService.CancelPaymentAsync(orderId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CancelPaymentAsync_ShouldReturnFalse_WhenPaymentIsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _paymentRepositoryMock
            .Setup(repo => repo.GetPaymentAsyncByOrderId(orderId))
            .ReturnsAsync(new Payments());

        // Act
        var result = await _paymentService.CancelPaymentAsync(orderId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddCardPaymentsAsync_ShouldReturnTrue_WhenPaymentAddedSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userName = "testUser";
        var sum = 100.0;
        var count = 2;

        _paymentRepositoryMock
            .Setup(repo => repo.AddPaymentAsync(It.IsAny<Payments>()))
            .ReturnsAsync(true);

        // Act
        var result = await _paymentService.AddCardPaymentsAsync(orderId, userName, sum, count);

        // Assert
        Assert.True(result);
        _paymentRepositoryMock.Verify(repo => repo.AddPaymentAsync(It.IsAny<Payments>()), Times.Once);
    }

    [Fact]
    public async Task AddCardPaymentsAsync_ShouldReturnFalse_WhenPaymentAddFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userName = "testUser";
        var sum = 100.0;
        var count = 2;

        _paymentRepositoryMock
            .Setup(repo => repo.AddPaymentAsync(It.IsAny<Payments>()))
            .ReturnsAsync(false);

        // Act
        var result = await _paymentService.AddCardPaymentsAsync(orderId, userName, sum, count);

        // Assert
        Assert.False(result);
    }
}