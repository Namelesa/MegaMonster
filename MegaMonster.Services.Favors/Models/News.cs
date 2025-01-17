namespace MegaMonster.Services.Favors.Models;

public class News(string type, string name, string description, string image, string link)
    : BaseModel
{
    public string Type { get; set; } = type;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string Image { get; set; } = image;
    public string Link { get; set; } = link;
}