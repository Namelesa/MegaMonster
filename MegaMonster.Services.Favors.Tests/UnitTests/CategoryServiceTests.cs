using MegaMonster.Services.Favors.Application.Category;
using MegaMonster.Services.Favors.Core.Category;
using MegaMonster.Services.Favors.Infrastructure.Redis;
using Moq;
using Category = MegaMonster.Services.Favors.Core.Category.Category;

namespace MegaMonster.Services.Favors.Tests.UnitTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<IRedisService> _mockRedisService;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockRedisService = new Mock<IRedisService>();
        _categoryService = new CategoryService(_mockCategoryRepository.Object, _mockRedisService.Object);
    }

    #region GetAllCategories Tests

    [Fact]
    public async Task GetAllCategories_WhenCacheHasData_ReturnsFromCache()
    {
        // Arrange
        var expectedCategories = new List<Category>
        {
            new("Electronics"),
            new("Books")
        };
        
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<Category>>("All_Categories"))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _categoryService.GetAllCategories();

        // Assert
        Assert.Equal(expectedCategories, result);
        _mockCategoryRepository.Verify(x => x.GetAll(), Times.Never);
    }

    [Fact]
    public async Task GetAllCategories_WhenCacheDoesNotHaveData_GetsFromRepositoryAndSetsCache()
    {
        // Arrange
        var expectedCategories = new List<Category>
        {
            new("Electronics"),
            new("Books")
        };
    
        _mockRedisService
            .Setup(x => x.GetAsync<IEnumerable<Category>>("All_Categories"))
            .ReturnsAsync((IEnumerable<Category>)null);
        
        _mockCategoryRepository
            .Setup(x => x.GetAll())
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _categoryService.GetAllCategories();

        // Assert
        Assert.Equal(expectedCategories, result);
    
        // Используем It.IsAny<IEnumerable<Category>>() вместо конкретного значения
        _mockRedisService.Verify(
            x => x.SetAsync(
                "All_Categories", 
                It.IsAny<IEnumerable<Category>>(), 
                It.IsAny<TimeSpan>()
            ), 
            Times.Once
        );
    }

    #endregion

    #region GetCategoryById Tests

    [Fact]
    public async Task GetCategoryById_WhenCacheHasData_ReturnsFromCache()
    {
        // Arrange
        var categoryId = 1;
        var expectedCategory = new Category("Books") { Id = categoryId };
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>($"Category_{categoryId}"))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await _categoryService.GetCategoryById(categoryId);

        // Assert
        Assert.Equal(expectedCategory, result);
        _mockCategoryRepository.Verify(x => x.GetCategoryById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCategoryById_WhenCacheDoesNotHaveData_GetsFromRepositoryAndSetsCache()
    {
        // Arrange
        var categoryId = 1;
        var expectedCategory = new Category("Books") { Id = categoryId };
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>($"Category_{categoryId}"))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.GetCategoryById(categoryId))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await _categoryService.GetCategoryById(categoryId);

        // Assert
        Assert.Equal(expectedCategory, result);
        _mockRedisService.Verify(
            x => x.SetAsync(
                $"Category_{categoryId}", 
                expectedCategory, 
                It.IsAny<TimeSpan>()
            ), 
            Times.Once
        );
    }

    #endregion

    #region GetCategoryByName Tests

    [Fact]
    public async Task GetCategoryByName_WhenCacheHasData_ReturnsFromCache()
    {
        // Arrange
        var categoryName = "Books";
        var expectedCategory = new Category(categoryName);
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>($"Category_{categoryName}"))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await _categoryService.GetCategoryByName(categoryName);

        // Assert
        Assert.Equal(expectedCategory, result);
        _mockCategoryRepository.Verify(x => x.GetCategoryByName(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCategoryByName_WhenCacheDoesNotHaveData_GetsFromRepositoryAndSetsCache()
    {
        // Arrange
        var categoryName = "Books";
        var expectedCategory = new Category(categoryName);
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>($"Category_{categoryName}"))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync(expectedCategory);

        // Act
        var result = await _categoryService.GetCategoryByName(categoryName);

        // Assert
        Assert.Equal(expectedCategory, result);
        _mockRedisService.Verify(
            x => x.SetAsync(
                $"Category_{categoryName}", 
                expectedCategory, 
                It.IsAny<TimeSpan>()
            ), 
            Times.Once
        );
    }

    #endregion

    #region AddCategory Tests

    [Fact]
    public async Task AddCategory_WithEmptyName_ReturnsFail()
    {
        // Arrange
        string emptyName = string.Empty;

        // Act
        var result = await _categoryService.AddCategory(emptyName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Category name cannot be empty.", result.Message);
        _mockCategoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task AddCategory_WhenCategoryAlreadyExists_ReturnsFail()
    {
        // Arrange
        var categoryName = "Books";
        var existingCategory = new Category(categoryName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _categoryService.AddCategory(categoryName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Category with name '{categoryName}' already exists.", result.Message);
        _mockCategoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task AddCategory_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        var categoryName = "Books";
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.AddCategory(categoryName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error adding category.", result.Message);
    }

    [Fact]
    public async Task AddCategory_WhenSuccessful_UpdatesCacheAndReturnsSuccess()
    {
        // Arrange
        var categoryName = "Books";
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.AddCategory(categoryName);

        // Assert
        Assert.True(result.Success);
        _mockRedisService.Verify(x => x.RemoveAsync("All_Categories"), Times.Once);
        _mockRedisService.Verify(x => x.RemoveAsync($"Category_{categoryName}"), Times.Once);
    }

    #endregion

    #region EditCategory Tests

    [Fact]
    public async Task EditCategory_WithEmptyNewName_ReturnsFail()
    {
        // Arrange
        var currentName = "Books";
        string newName = string.Empty;

        // Act
        var result = await _categoryService.EditCategory(currentName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("New category name cannot be empty.", result.Message);
        _mockCategoryRepository.Verify(x => x.EditAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task EditCategory_WhenCurrentCategoryNotFound_ReturnsFail()
    {
        // Arrange
        var currentName = "Books";
        var newName = "eBooks";
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(currentName))
            .ReturnsAsync((Category)null);

        // Act
        var result = await _categoryService.EditCategory(currentName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Category '{currentName}' not found.", result.Message);
    }

    [Fact]
    public async Task EditCategory_WhenNewNameAlreadyExists_ReturnsFail()
    {
        // Arrange
        var currentName = "Books";
        var newName = "eBooks";
        var currentCategory = new Category(currentName);
        var existingCategory = new Category(newName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(currentName))
            .ReturnsAsync(currentCategory);
            
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(newName))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _categoryService.EditCategory(currentName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Category with name '{newName}' already exists.", result.Message);
    }

    [Fact]
    public async Task EditCategory_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        var currentName = "Books";
        var newName = "eBooks";
        var currentCategory = new Category(currentName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(currentName))
            .ReturnsAsync(currentCategory);
            
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(newName))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.EditAsync(It.IsAny<Category>()))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.EditCategory(currentName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error updating category.", result.Message);
    }

    [Fact]
    public async Task EditCategory_WhenSuccessful_UpdatesCacheAndReturnsSuccess()
    {
        // Arrange
        var currentName = "Books";
        var newName = "eBooks";
        var currentCategory = new Category(currentName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(currentName))
            .ReturnsAsync(currentCategory);
            
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(newName))
            .ReturnsAsync((Category)null);
            
        _mockCategoryRepository
            .Setup(x => x.EditAsync(It.IsAny<Category>()))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.EditCategory(currentName, newName);

        // Assert
        Assert.True(result.Success);
        _mockRedisService.Verify(x => x.RemoveAsync("All_Categories"), Times.Once);
        _mockRedisService.Verify(x => x.RemoveAsync($"Category_{currentName}"), Times.Once);
        _mockRedisService.Verify(x => x.RemoveAsync($"Category_{newName}"), Times.Once);
    }

    #endregion

    #region DeleteCategory Tests

    [Fact]
    public async Task DeleteCategory_WhenCategoryNotFound_ReturnsFail()
    {
        // Arrange
        var categoryName = "Books";
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync((Category)null);

        // Act
        var result = await _categoryService.DeleteCategory(categoryName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Category '{categoryName}' not found.", result.Message);
    }

    [Fact]
    public async Task DeleteCategory_WhenRepositoryFails_ReturnsFail()
    {
        // Arrange
        var categoryName = "Books";
        var category = new Category(categoryName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync(category);
            
        _mockCategoryRepository
            .Setup(x => x.DeleteAsync(category))
            .ReturnsAsync(false);

        // Act
        var result = await _categoryService.DeleteCategory(categoryName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error deleting category.", result.Message);
    }

    [Fact]
    public async Task DeleteCategory_WhenSuccessful_UpdatesCacheAndReturnsSuccess()
    {
        // Arrange
        var categoryName = "Books";
        var category = new Category(categoryName);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync(category);
            
        _mockCategoryRepository
            .Setup(x => x.DeleteAsync(category))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.DeleteCategory(categoryName);

        // Assert
        Assert.True(result.Success);
        _mockRedisService.Verify(x => x.RemoveAsync("All_Categories"), Times.Once);
        _mockRedisService.Verify(x => x.RemoveAsync($"Category_{categoryName}"), Times.Once);
    }

    #endregion

    #region GetOrSetCache Tests

    [Fact]
    public async Task GetOrSetCache_WhenCacheHasData_ReturnsFromCacheWithoutCallingGetData()
    {
        // Arrange
        var categoryName = "Test";
        var cachedValue = new Category(categoryName);
        var key = $"Category_{categoryName}";
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>(key))
            .ReturnsAsync(cachedValue);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .Callback(() => Assert.True(false, "Repository should not be called when data is in cache"))
            .ReturnsAsync(new Category("Different"));

        // Act
        var result = await _categoryService.GetCategoryByName(categoryName);

        // Assert
        Assert.Equal(cachedValue, result);
        
        _mockRedisService.Verify(x => x.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<object>(),
                It.IsAny<TimeSpan>()), 
            Times.Never);
        
        _mockCategoryRepository.Verify(x => x.GetCategoryByName(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrSetCache_WhenCacheDoesNotHaveData_CallsGetDataAndSetsCache()
    {
        // Arrange
        var categoryName = "Test";
        var expectedData = new Category(categoryName);
        var key = $"Category_{categoryName}";
        
        _mockRedisService
            .Setup(x => x.GetAsync<Category>(key))
            .ReturnsAsync((Category)null);
        
        _mockCategoryRepository
            .Setup(x => x.GetCategoryByName(categoryName))
            .ReturnsAsync(expectedData);

        // Act 
        var result = await _categoryService.GetCategoryByName(categoryName);

        // Assert
        Assert.Equal(expectedData, result);
        
        _mockRedisService.Verify(x => x.SetAsync(
                key, 
                It.IsAny<Category>(),
                It.IsAny<TimeSpan>()), 
            Times.Once);
    }
    
    private async Task<T> InvokeGetOrSetCache<T>(string key, Func<Task<T>> getData, TimeSpan? expiration = null)
    {
        if (typeof(T) == typeof(Category))
        {
            var name = key.Replace("Category_", "");
            return (T)(object)await _categoryService.GetCategoryByName(name);
        }
        else if (typeof(T) == typeof(IEnumerable<Category>))
        {
            return (T)(object)await _categoryService.GetAllCategories();
        }
        
        throw new NotSupportedException("Test this type does not support");
    }

    #endregion
}