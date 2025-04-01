namespace MegaMonster.Services.Favors.Core.Interfaces;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAll();
    Task<bool> AddAsync(T t);
    Task<bool> EditAsync(T t);
    Task<bool> DeleteAsync(T t);
}