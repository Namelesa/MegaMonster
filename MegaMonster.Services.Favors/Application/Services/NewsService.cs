using MegaMonster.Services.Favors.Application.OperationResult;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Application.Services;

public class NewsService(INewsRepository newsRepository)
{
    public async Task<IEnumerable<News>> GetAllNews() => await newsRepository.GetAll();

    public async Task<ResultOperation> AddNews(News news)
    {
        if (string.IsNullOrWhiteSpace(news.Name)) 
            return ResultOperation.Fail("News name cannot be empty.");
        
        return await newsRepository.AddAsync(news)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error adding news.");
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
        return await newsRepository.EditAsync(currentNews)
            ? ResultOperation.Ok()
            : ResultOperation.Fail("Error updating news.");
    }

    public async Task<ResultOperation> DeleteNews(int id)
    {
        var news = await newsRepository.GetNewsById(id);
        if (news == null) return ResultOperation.Fail("Not found news with this id");

        var result = await newsRepository.DeleteAsync(news);
        return result ? ResultOperation.Ok() : ResultOperation.Fail("Can not delete news");
    }
}