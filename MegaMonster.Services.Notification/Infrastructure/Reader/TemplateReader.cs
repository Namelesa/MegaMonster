namespace MegaMonster.Services.Notification.Infrastructure.Reader;

public class TemplateReader(IWebHostEnvironment webHostEnvironment) : ITemplateReader
{
    public async Task<string?> ReadTemplateAsync(string templatePath)
    {
        var pathToTemplate = Path.Combine(webHostEnvironment.ContentRootPath, templatePath);

        if (!File.Exists(pathToTemplate))
        {
            return null;
        }

        using var sr = File.OpenText(pathToTemplate);
        return await sr.ReadToEndAsync();
    }
}