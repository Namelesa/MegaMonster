namespace MegaMonster.Services.Auth.Core.Interfaces;

public interface IRepository<T>
{
    Task<bool> CheckLoginAndEmail(string login, string email);
}