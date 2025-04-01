using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Category;

public class Category(string name) : BaseModel
{
    [Required]
    public string Name { get; set; } = name;
}