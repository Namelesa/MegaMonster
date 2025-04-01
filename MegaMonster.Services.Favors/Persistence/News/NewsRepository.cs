using MegaMonster.Services.Favors.Core.News;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.News;

public class NewsRepository(AppDbContext db) : INewsRepository
{
    public async Task<IEnumerable<Core.News.News>> GetAll()
    {
        return await db.News.ToListAsync<Core.News.News>();
    }

    public async Task<bool> AddAsync(Core.News.News t)
    {
        try
        {
            await db.News.AddAsync(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> EditAsync(Core.News.News t)
    {
        try
        {
            db.News.Update(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Core.News.News t)
    {
        try
        {
            db.News.Remove(t);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<Core.News.News?> GetNewsById(int id)
    {
        return await db.News.AsNoTracking<Core.News.News>().FirstOrDefaultAsync(c => c.Id == id);
    }
}