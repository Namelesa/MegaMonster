using MegaMonster.Services.Favors.Core.Category;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Category;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IEnumerable<Core.Category.Category>> GetAll() => await db.Categories.AsNoTracking<Core.Category.Category>().ToListAsync();

    public async Task<Core.Category.Category?> GetCategoryById(int id) => 
        await db.Categories.AsNoTracking<Core.Category.Category>().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Core.Category.Category?> GetCategoryByName(string name) => 
        await db.Categories.AsNoTracking<Core.Category.Category>().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<bool> AddAsync(Core.Category.Category category)
    {
        try
        {
            await db.Categories.AddAsync(category);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding category: {e.Message}");
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.Category.Category category)
    {
        try
        {
            db.Categories.Update(category);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating category: {e.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Core.Category.Category category)
    {
        try
        {
            db.Categories.Remove(category);
            return await db.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting category: {e.Message}");
            return false;
        }
    }
}