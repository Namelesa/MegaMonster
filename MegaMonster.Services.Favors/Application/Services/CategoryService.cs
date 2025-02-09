using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Infrastructure;
using MegaMonster.Services.Favors.Infrastructure.Redis;

namespace MegaMonster.Services.Favors.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository, IRedisService redisService)
{
    private const string CategoryCacheKey = "All_Categories";

    public async Task<IEnumerable<Category>> GetAllCategories() => 
        await GetOrSetCache(CategoryCacheKey, () => categoryRepository.GetAll()!);

    public async Task<Category?> GetCategoryById(int id) => 
        await GetOrSetCache($"Category_{id}", () => categoryRepository.GetCategoryById(id));

    public async Task<Category?> GetCategoryByName(string name) => 
        await GetOrSetCache($"Category_{name}", () => categoryRepository.GetCategoryByName(name));

    public async Task<ResultOperation> AddCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) 
            return ResultOperation.Fail("Category name cannot be empty.");

        if (await categoryRepository.GetCategoryByName(categoryName) is not null)
            return ResultOperation.Fail($"Category with name '{categoryName}' already exists.");

        var category = new Category(categoryName);
        var result = await categoryRepository.AddAsync(category);
        if (!result) return ResultOperation.Fail("Error adding category.");
        
        await ProcessChange(categoryName);
        return ResultOperation.Ok();
    }

    public async Task<ResultOperation> EditCategory(string currentName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return ResultOperation.Fail("New category name cannot be empty.");

        var currentCategory = await categoryRepository.GetCategoryByName(currentName);
        if (currentCategory is null)
            return ResultOperation.Fail($"Category '{currentName}' not found.");

        if (await categoryRepository.GetCategoryByName(newName) is not null)
            return ResultOperation.Fail($"Category with name '{newName}' already exists.");

        currentCategory.Name = newName;
        var result = await categoryRepository.EditAsync(currentCategory);
        if (!result) return ResultOperation.Fail("Error updating category.");
        
        await ProcessChange(currentName, newName);
        return ResultOperation.Ok();
    }

    public async Task<ResultOperation> DeleteCategory(string categoryName)
    {
        var category = await categoryRepository.GetCategoryByName(categoryName);
        if (category is null)
            return ResultOperation.Fail($"Category '{categoryName}' not found.");

        var result = await categoryRepository.DeleteAsync(category);
        if (!result) return ResultOperation.Fail("Error deleting category.");
        
        await ProcessChange(categoryName);
        return ResultOperation.Ok();
    }

    private async Task<T?> GetOrSetCache<T>(string key, Func<Task<T?>> getData, TimeSpan? expiration = null)
    {
        var cachedData = await redisService.GetAsync<T>(key);
        if (cachedData is not null) return cachedData;
        
        var data = await getData();
        if (data is not null)
            await redisService.SetAsync(key, data, expiration ?? TimeSpan.FromMinutes(60));
        
        return data;
    }
    
    private async Task ProcessChange(params string[] categoryNames)
    {
        await redisService.RemoveAsync(CategoryCacheKey);
        foreach (var categoryName in categoryNames)
        {
            await redisService.RemoveAsync($"Category_{categoryName}");
        }
    }
}
