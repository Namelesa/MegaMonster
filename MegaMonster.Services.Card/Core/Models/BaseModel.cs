using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Card.Core.Models;

public class BaseModel
{
    [Key]
    public int Id { get; set; }
}