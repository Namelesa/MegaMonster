using Microsoft.AspNetCore.Hosting;
using Moq;

namespace MegaMonster.Services.Notification.Tests.UnitTests.TemplateReader;

public class TemplateReaderTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    [Fact]
    public async Task ReadTemplateAsync_ReturnsContent_WhenFileExists()
    {
        // Arrange
        var contentRoot = Path.GetTempPath();
        var fileName = "testTemplate.txt";
        var filePath = Path.Combine(contentRoot, fileName);
        var expectedContent = "Hello from template!";

        await File.WriteAllTextAsync(filePath, expectedContent);

        _envMock.Setup(e => e.ContentRootPath).Returns(contentRoot);
        var reader = new Infrastructure.Reader.TemplateReader(_envMock.Object);

        // Act
        var result = await reader.ReadTemplateAsync(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedContent, result);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task ReadTemplateAsync_ReturnsNull_WhenFileDoesNotExist()
    {
        // Arrange
        var contentRoot = Path.GetTempPath();
        var nonExistentFile = "does-not-exist.txt";

        _envMock.Setup(e => e.ContentRootPath).Returns(contentRoot);
        var reader = new Infrastructure.Reader.TemplateReader(_envMock.Object);

        // Act
        var result = await reader.ReadTemplateAsync(nonExistentFile);

        // Assert
        Assert.Null(result);
    }
}