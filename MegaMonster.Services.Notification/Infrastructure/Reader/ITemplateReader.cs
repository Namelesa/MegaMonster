namespace MegaMonster.Services.Notification.Infrastructure.Reader;

public interface ITemplateReader
{
    Task<string?> ReadTemplateAsync(string templatePath);
}