using MegaMonster.Services.Favors.Core.Interfaces;

namespace MegaMonster.Services.Favors.Core.News;

public interface INewsRepository : IRepository<News>
{ 
    Task<News?> GetNewsById(int id);
}