namespace MegaMonster.Services.Auth.Application.Interfaces;

public interface IRepository<T>
{
    Task<bool> CheckLoginAndEmail(string login, string email);
}