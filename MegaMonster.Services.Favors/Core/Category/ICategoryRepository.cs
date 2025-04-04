using MegaMonster.Services.Favors.Core.BaseModel;

namespace MegaMonster.Services.Favors.Core.Category;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Core.Category.Category?> GetCategoryById(int id);
    Task<Core.Category.Category?> GetCategoryByName(string name);
}