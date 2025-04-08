using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace MegaMonster.Services.Notification.Tests.UnitTests.EmailSender;

public class EmailSenderTests
{
    private readonly Mock<ILogger<Infrastructure.MailJet.EmailSender>> _loggerMock = new();

    private IConfiguration SetupConfig(bool withValidSettings)
    {
        var configData = new Dictionary<string, string>();

        if (withValidSettings)
        {
            configData = new Dictionary<string, string>
            {
                { "MailJet:ApiKey", "valid-api-key" },
                { "MailJet:SecretKey", "valid-secret" }
            };
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public async Task SendEmailAsync_WithNullSettings_DoesNotThrow()
    {
        // Arrange
        var config = SetupConfig(false);
        var sender = new Infrastructure.MailJet.EmailSender(config, _loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            sender.SendEmailAsync("test@example.com", "Subject", "<p>Hello</p>")
        );

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_WithInvalidApiKey_LogsError()
    {
        // Arrange
        var config = SetupConfig(true);
        var sender = new Infrastructure.MailJet.EmailSender(config, _loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            sender.SendEmailAsync("test@example.com", "Subject", "<p>Hello</p>")
        );

        // Assert
        Assert.Null(exception);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

}
