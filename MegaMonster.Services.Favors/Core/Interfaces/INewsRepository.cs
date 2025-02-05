using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface INewsRepository : IRepository<News>
{ 
    Task<News?> GetNewsById(int id);
}