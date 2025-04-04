using MegaMonster.Services.Favors.Core.BaseModel;

namespace MegaMonster.Services.Favors.Core.News;

public interface INewsRepository : IRepository<News>
{ 
    Task<News?> GetNewsById(int id);
}