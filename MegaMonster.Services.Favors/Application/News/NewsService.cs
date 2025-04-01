using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.News;
using MegaMonster.Services.Favors.Infrastructure.Redis;

namespace MegaMonster.Services.Favors.Application.News;

public class NewsService(INewsRepository newsRepository, IRedisService redisService)
{
    private const string NewsCacheKey = "All_News";

    public async Task<IEnumerable<Core.News.News>> GetAllNews() => 
        await GetOrSetCache(NewsCacheKey, () => newsRepository.GetAll()!);

    public async Task<ResultOperation> AddNews(Core.News.News news)
    {
        if (string.IsNullOrWhiteSpace(news.Name)) 
            return ResultOperation.Fail("News name cannot be empty.");
        
        var result = await newsRepository.AddAsync(news);
        if (!result) return ResultOperation.Fail("Error adding news.");
        
        await ProcessChange();
        return ResultOperation.Ok();
    }
    
    public async Task<ResultOperation> EditNews(int id, string type, string name, string description, string image, string link)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ResultOperation.Fail("New news name cannot be empty.");

        var currentNews = await newsRepository.GetNewsById(id);
        if (currentNews is null)
            return ResultOperation.Fail("News not found.");
        
        currentNews.Name = name;
        currentNews.Type = type;
        currentNews.Description = description;
        currentNews.Image = image;
        currentNews.Link = link;
        
        var result = await newsRepository.EditAsync(currentNews);
        if (!result) return ResultOperation.Fail("Error updating news.");
        
        await ProcessChange();
        return ResultOperation.Ok();
    }

    public async Task<ResultOperation> DeleteNews(int id)
    {
        var news = await newsRepository.GetNewsById(id);
        if (news == null) return ResultOperation.Fail("Not found news with this id");

        var result = await newsRepository.DeleteAsync(news);
        if (!result) return ResultOperation.Fail("Can not delete news");
        
        await ProcessChange();
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
    
    private async Task ProcessChange()
    {
        await redisService.RemoveAsync(NewsCacheKey);
    }
}
