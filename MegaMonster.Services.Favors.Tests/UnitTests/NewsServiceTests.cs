using MegaMonster.Services.Favors.Application.News;
using MegaMonster.Services.Favors.Core.News;
using MegaMonster.Services.Favors.Infrastructure.Redis;
using Moq;

namespace MegaMonster.Services.Favors.Tests.UnitTests;

public class NewsServiceTests
{
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly NewsService _newsService;

    public NewsServiceTests()
    {
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockRedisService = new Mock<IRedisService>();
        _newsService = new NewsService(_mockNewsRepository.Object, _mockRedisService.Object);
    }

    #region GetAllNews Tests

    [Fact]
    public async Task GetAllNews_WhenCacheHasData_ReturnsFromCache()
    {
        // Arrange
        var expectedNews = new List<News>
        {
            new("Type1", "News 1", "Description 1", "1", "1"),
            new("Type2", "News 2", "Description 2", "2", "2")
        };
        
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<News>>("All_News"))
            .ReturnsAsync(expectedNews);

        // Act
        var result = await _newsService.GetAllNews();

        // Assert
        Assert.Equal(expectedNews, result);
        _mockNewsRepository.Verify(x => x.GetAll(), Times.Never);
    }

    [Fact]
    public async Task GetAllNews_WhenCacheDoesNotHaveData_GetsFromRepositoryAndSetsCache()
    {
        // Arrange
        var expectedNews = new List<News>
        {
            new("Type1", "News 1", "Description 1", "1", "1"),
            new("Type2", "News 2", "Description 2", "2", "2")
        };
        
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<News>>("All_News"))
            .ReturnsAsync((IEnumerable<News>)null);
            
        _mockNewsRepository
            .Setup(x => x.GetAll())
            .ReturnsAsync(expectedNews);

        // Act
        var result = await _newsService.GetAllNews();

        // Assert
        Assert.Equal(expectedNews, result);
        _mockRedisService.Verify(
            x => x.SetAsync(
                "All_News", 
                It.IsAny<IEnumerable<News>>(), 
                It.IsAny<TimeSpan>()
            ), 
            Times.Once
        );
    }

    #endregion

    #region AddNews Tests

    [Fact]
    public async Task AddNews_WithEmptyName_ReturnsFail()
    {
        // Arrange
        var news = new News("Type1", "", "Description 1", "1", "1");

        // Act
        var result = await _newsService.AddNews(news);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("News name cannot be empty.", result.Message);
        _mockNewsRepository.Verify(x => x.AddAsync(It.IsAny<News>()), Times.Never);
    }

    [Fact]
    public async Task AddNews_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        var news = new News("Type1", "News 1", "Description 1", "1", "1");
            
        _mockNewsRepository
            .Setup(x => x.AddAsync(news))
            .ReturnsAsync(false);

        // Act
        var result = await _newsService.AddNews(news);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error adding news.", result.Message);
    }

    [Fact]
    public async Task AddNews_WhenSuccessful_ClearsNewsCache()
    {
        // Arrange
        var news = new News("Type1", "News 1", "Description 1", "1", "1");
            
        _mockNewsRepository
            .Setup(x => x.AddAsync(news))
            .ReturnsAsync(true);

        // Act
        var result = await _newsService.AddNews(news);

        // Assert
        Assert.True(result.Success);
        _mockRedisService.Verify(x => x.RemoveAsync("All_News"), Times.Once);
    }

    #endregion

    #region EditNews Tests

    [Fact]
    public async Task EditNews_WithEmptyName_ReturnsFail()
    {
        // Arrange
        var id = 1;
        var type = "Type1";
        var name = "";
        var description = "Updated Description";
        var image = "image.jpg";
        var link = "http://example.com";

        // Act
        var result = await _newsService.EditNews(id, type, name, description, image, link);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("New news name cannot be empty.", result.Message);
    }

    [Fact]
    public async Task EditNews_WhenNewsNotFound_ReturnsFail()
    {
        // Arrange
        int id = 1;
        string type = "Type1";
        string name = "Updated News";
        string description = "Updated Description";
        string image = "image.jpg";
        string link = "http://example.com";
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync((News)null);

        // Act
        var result = await _newsService.EditNews(id, type, name, description, image, link);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("News not found.", result.Message);
    }

    [Fact]
    public async Task EditNews_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        int id = 1;
        string type = "Type1";
        string name = "Updated News";
        string description = "Updated Description";
        string image = "image.jpg";
        string link = "http://example.com";
        
        var existingNews = new News( "Type1", "News 1", "Description 1", "1", "1")
        { 
            Id = id, 
            Name = "Original News", 
            Type = "OriginalType", 
            Description = "Original Description" 
        };
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync(existingNews);
            
        _mockNewsRepository
            .Setup(x => x.EditAsync(It.IsAny<Core.News.News>()))
            .ReturnsAsync(false);

        // Act
        var result = await _newsService.EditNews(id, type, name, description, image, link);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error updating news.", result.Message);
    }

    [Fact]
    public async Task EditNews_WhenSuccessful_UpdatesNewsAndClearsCache()
    {
        // Arrange
        int id = 1;
        string type = "Type1";
        string name = "Updated News";
        string description = "Updated Description";
        string image = "image.jpg";
        string link = "http://example.com";
        
        var existingNews = new News( "Type1", "News 1", "Description 1", "1", "1")
        { 
            Id = id, 
            Name = "Original News", 
            Type = "OriginalType", 
            Description = "Original Description" 
        };
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync(existingNews);
            
        _mockNewsRepository
            .Setup(x => x.EditAsync(It.IsAny<Core.News.News>()))
            .ReturnsAsync(true);

        // Act
        var result = await _newsService.EditNews(id, type, name, description, image, link);

        // Assert
        Assert.True(result.Success);
        
        // Verify news properties were updated
        Assert.Equal(name, existingNews.Name);
        Assert.Equal(type, existingNews.Type);
        Assert.Equal(description, existingNews.Description);
        Assert.Equal(image, existingNews.Image);
        Assert.Equal(link, existingNews.Link);
        
        // Verify cache was cleared
        _mockRedisService.Verify(x => x.RemoveAsync("All_News"), Times.Once);
    }

    #endregion

    #region DeleteNews Tests

    [Fact]
    public async Task DeleteNews_WhenNewsNotFound_ReturnsFail()
    {
        // Arrange
        int id = 1;
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync((News)null);

        // Act
        var result = await _newsService.DeleteNews(id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Not found news with this id", result.Message);
    }

    [Fact]
    public async Task DeleteNews_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        int id = 1;
        var existingNews = new News("Test","Test News", "Test", "1", "1");
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync(existingNews);
            
        _mockNewsRepository
            .Setup(x => x.DeleteAsync(existingNews))
            .ReturnsAsync(false);

        // Act
        var result = await _newsService.DeleteNews(id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Can not delete news", result.Message);
    }

    [Fact]
    public async Task DeleteNews_WhenSuccessful_ClearsNewsCache()
    {
        // Arrange
        int id = 1;
        var existingNews = new News("Test","Test News", "Test", "1", "1");
        
        _mockNewsRepository
            .Setup(x => x.GetNewsById(id))
            .ReturnsAsync(existingNews);
            
        _mockNewsRepository
            .Setup(x => x.DeleteAsync(existingNews))
            .ReturnsAsync(true);

        // Act
        var result = await _newsService.DeleteNews(id);

        // Assert
        Assert.True(result.Success);
        _mockRedisService.Verify(x => x.RemoveAsync("All_News"), Times.Once);
    }

    #endregion

    #region GetOrSetCache Tests

    [Fact]
    public async Task GetOrSetCache_WhenCacheHasData_ReturnsFromCacheWithoutCallingGetData()
    {
        // Arrange
        var cachedNews = new List<News>
        {
            new("Type1", "News 1", "Description 1", "1", "1"),
            new("Type2", "News 2", "Description 2", "2", "2")
        };
        
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<News>>("All_News"))
            .ReturnsAsync(cachedNews);

        // Setup repository to fail if called - it shouldn't be called in this scenario
        _mockNewsRepository
            .Setup(x => x.GetAll())
            .Callback(() => Assert.True(false, "Repository should not be called when data is in cache"))
            .ReturnsAsync(new List<News>());

        // Act
        var result = await _newsService.GetAllNews();

        // Assert
        Assert.Equal(cachedNews, result);
        _mockRedisService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            It.IsAny<object>(),
            It.IsAny<TimeSpan>()), 
            Times.Never);
        _mockNewsRepository.Verify(x => x.GetAll(), Times.Never);
    }

    [Fact]
    public async Task GetOrSetCache_WhenCacheDoesNotHaveData_CallsGetDataAndSetsCache()
    {
        // Arrange
        var expectedNews = new List<News>
        {
            new("Type1", "News 1", "Description 1", "1", "1"),
            new("Type2", "News 2", "Description 2", "2", "2")
        };
        
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<News>>("All_News"))
            .ReturnsAsync((IEnumerable<News>)null);
            
        _mockNewsRepository
            .Setup(x => x.GetAll())
            .ReturnsAsync(expectedNews);

        // Act
        var result = await _newsService.GetAllNews();

        // Assert
        Assert.Equal(expectedNews, result);
        _mockRedisService.Verify(x => x.SetAsync(
            "All_News", 
            It.IsAny<IEnumerable<News>>(),
            It.IsAny<TimeSpan>()), 
            Times.Once);
    }

    #endregion
}