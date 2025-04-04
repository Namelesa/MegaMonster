using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Core.Category;

public class Category(string name) : BaseModel.BaseModel
{
    [Required]
    public string Name { get; set; } = name;
}