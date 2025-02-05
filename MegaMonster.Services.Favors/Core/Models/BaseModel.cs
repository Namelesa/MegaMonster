using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Core.Models;

public class BaseModel
{
    [Key]
    public int Id { get; set; }
}