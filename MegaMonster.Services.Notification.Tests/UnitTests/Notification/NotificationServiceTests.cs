using FluentAssertions;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.User;
using MegaMonster.Services.Notification.Infrastructure.MailJet;
using MegaMonster.Services.Notification.Infrastructure.Reader;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;

namespace MegaMonster.Services.Notification.Tests.UnitTests.Notification;

public class NotificationServiceTests
{
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<ITemplateReader> _mockTemplateReader;
    private readonly INotification _notificationService;

    public NotificationServiceTests()
    {
        _mockEmailSender = new Mock<IEmailSender>();
        _mockTemplateReader = new Mock<ITemplateReader>();
        _notificationService = new Persistence.Notification(_mockEmailSender.Object, _mockTemplateReader.Object);
    }

    [Fact]
    public async Task SendConfirmEmailAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com", "https://example.com/confirm");
        var templateContent = "Hello {name}, please confirm your email {email} by clicking {link}";
        var expectedContent = "Hello TestUser, please confirm your email test@example.com by clicking https://example.com/confirm";

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/ConfirmRegister.html"))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _notificationService.SendConfirmEmailAsync(userDto);

        // Assert
        result.Should().BeTrue();
        _mockEmailSender.Verify(
            s => s.SendEmailAsync(
                userDto.Email,
                Wc.ConfirmEmail,
                It.Is<string>(body => body == expectedContent)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendConfirmEmailAsync_WithNullTemplate_ReturnsFalse()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com", "https://example.com/confirm");

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/ConfirmRegister.html"))
            .ReturnsAsync((string)null);

        // Act
        var result = await _notificationService.SendConfirmEmailAsync(userDto);

        // Assert
        result.Should().BeFalse();
        _mockEmailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendBillEmailAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var billUserDto = new BillUserDto
        {
            UserName = "TestUser",
            Email = "test@example.com",
            OrderId = orderId,
            PaymentType = "Credit Card",
            OrderDetailsRows = new List<int> { 1, 2, 3, 4 },
            Sum = 25.00,
            Status = "Completed"
        };

        var templateContent = "Hello {UserName}, your order {OrderId} with {PaymentType} for {Email} has items: {OrderDetailsRows} totaling {Sum} with status {Status}";

        var orderDetailsText = string.Join("<br>", billUserDto.OrderDetailsRows);
        var expectedContent = templateContent
            .Replace("{UserName}", billUserDto.UserName)
            .Replace("{OrderId}", billUserDto.OrderId.ToString())
            .Replace("{PaymentType}", billUserDto.PaymentType)
            .Replace("{Email}", billUserDto.Email)
            .Replace("{OrderDetailsRows}", orderDetailsText)
            .Replace("{Sum}", billUserDto.Sum.ToString())
            .Replace("{Status}", billUserDto.Status);

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/Bill.html"))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _notificationService.SendBillEmailAsync(billUserDto);

        // Assert
        result.Should().BeTrue();
        _mockEmailSender.Verify(
            s => s.SendEmailAsync(
                billUserDto.Email,
                Wc.Information,
                It.Is<string>(body => body == expectedContent)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendBillEmailAsync_WithNullTemplate_ReturnsFalse()
    {
        // Arrange
        var billUserDto = new BillUserDto
        {
            UserName = "TestUser",
            Email = "test@example.com"
        };

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/Bill.html"))
            .ReturnsAsync((string)null);

        // Act
        var result = await _notificationService.SendBillEmailAsync(billUserDto);

        // Assert
        result.Should().BeFalse();
        _mockEmailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendBanEmailAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com");
        var banReason = "Violation of terms of service";
        var templateContent = "Hello {name}, your account {email} has been banned for: {reason}";
        var expectedContent = "Hello TestUser, your account test@example.com has been banned for: Violation of terms of service";

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/BanUser.html"))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _notificationService.SendBanEmailAsync(userDto, banReason);

        // Assert
        result.Should().BeTrue();
        _mockEmailSender.Verify(
            s => s.SendEmailAsync(
                userDto.Email,
                Wc.BlockedUser,
                It.Is<string>(body => body == expectedContent)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendBanEmailAsync_WithNullTemplate_ReturnsFalse()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com");
        var banReason = "Violation of terms of service";

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/BanUser.html"))
            .ReturnsAsync((string)null);

        // Act
        var result = await _notificationService.SendBanEmailAsync(userDto, banReason);

        // Assert
        result.Should().BeFalse();
        _mockEmailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNewsEmailAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com");
        var templateContent = "Latest news from our platform";

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/News.html"))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _notificationService.SendNewsEmailAsync(userDto);

        // Assert
        result.Should().BeTrue();
        _mockEmailSender.Verify(
            s => s.SendEmailAsync(
                userDto.Email,
                Wc.News,
                It.Is<string>(body => body == templateContent)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendNewsEmailAsync_WithNullTemplate_ReturnsFalse()
    {
        // Arrange
        var userDto = new UserDto("TestUser", "test@example.com");

        _mockTemplateReader
            .Setup(r => r.ReadTemplateAsync("Infrastructure/Templates/News.html"))
            .ReturnsAsync((string)null);

        // Act
        var result = await _notificationService.SendNewsEmailAsync(userDto);

        // Assert
        result.Should().BeFalse();
        _mockEmailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}