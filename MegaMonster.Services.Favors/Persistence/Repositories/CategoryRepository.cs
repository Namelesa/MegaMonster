using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAll() => await db.Categories.AsNoTracking().ToListAsync();

    public async Task<Category?> GetCategoryById(int id) => 
        await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category?> GetCategoryByName(string name) => 
        await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<bool> AddAsync(Category category)
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

    public async Task<bool> EditAsync(Category category)
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

    public async Task<bool> DeleteAsync(Category category)
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