using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Repositories;

public class NewsRepository(AppDbContext db) : INewsRepository
{
    public async Task<IEnumerable<News>> GetAll()
    {
        return await db.News.ToListAsync();
    }

    public async Task<bool> AddAsync(News t)
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

    public async Task<bool> EditAsync(News t)
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

    public async Task<bool> DeleteAsync(News t)
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
    
    public async Task<News?> GetNewsById(int id)
    {
        return await db.News.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }
}