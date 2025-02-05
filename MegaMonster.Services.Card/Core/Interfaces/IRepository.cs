namespace MegaMonster.Services.Card.Core.Interfaces;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<bool> AddAsync(T t);
    Task<bool> EditAsync(T t);
    Task<bool> DeleteAsync(T t);
}