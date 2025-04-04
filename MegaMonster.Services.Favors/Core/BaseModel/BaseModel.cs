using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Core.BaseModel;

public class BaseModel
{
    [Key]
    public int Id { get; set; }
}