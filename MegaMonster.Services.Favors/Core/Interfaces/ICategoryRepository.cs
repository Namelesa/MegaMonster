using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetCategoryById(int id);
    Task<Category?> GetCategoryByName(string name);
}