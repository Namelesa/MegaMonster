using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Models;

public class Category(string name) : BaseModel
{
    [Required]
    public string Name { get; set; } = name;
}