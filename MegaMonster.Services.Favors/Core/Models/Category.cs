using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Core.Models;

public class Category(string name) : BaseModel
{
    [Required]
    public string Name { get; set; } = name;
}