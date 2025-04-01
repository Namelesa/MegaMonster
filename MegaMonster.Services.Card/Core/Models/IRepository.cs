namespace MegaMonster.Services.Card.Core.Models;

public interface IRepository<T>
{
    Task<bool> AddAsync(T t);
    Task<bool> EditAsync(T t);
    Task<bool> DeleteAsync(T t);
}