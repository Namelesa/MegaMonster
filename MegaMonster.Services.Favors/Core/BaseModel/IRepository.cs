namespace MegaMonster.Services.Favors.Core.BaseModel;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAll();
    Task<bool> AddAsync(T t);
    Task<bool> EditAsync(T t);
    Task<bool> DeleteAsync(T t);
}