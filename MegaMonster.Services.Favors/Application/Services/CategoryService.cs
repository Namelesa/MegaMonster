using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository)
{
    public async Task<IEnumerable<Category>> GetAllCategories() => await categoryRepository.GetAll();

    public async Task<Category?> GetCategoryById(int id) => await categoryRepository.GetCategoryById(id);

    public async Task<Category?> GetCategoryByName(string name) => await categoryRepository.GetCategoryByName(name);

    public async Task<ResultOperation> AddCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) 
            return ResultOperation.Fail("Category name cannot be empty.");

        var existingCategory = await categoryRepository.GetCategoryByName(categoryName);
        if (existingCategory is not null)
            return ResultOperation.Fail($"Category with name '{categoryName}' already exists.");

        var category = new Category(categoryName);
        return await categoryRepository.AddAsync(category)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error adding category.");
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
        return await categoryRepository.EditAsync(currentCategory)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error updating category.");
    }

    public async Task<ResultOperation> DeleteCategory(string categoryName)
    {
        var category = await categoryRepository.GetCategoryByName(categoryName);
        if (category is null)
            return ResultOperation.Fail($"Category '{categoryName}' not found.");

        return await categoryRepository.DeleteAsync(category)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error deleting category.");
    }
}
