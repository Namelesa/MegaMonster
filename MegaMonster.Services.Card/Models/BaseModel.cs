using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Card.Models;

public class BaseModel
{
    [Key]
    public int Id { get; set; }
}