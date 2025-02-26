using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Card.Core.Models;

public class BaseModel
{
    [Key]
    public Guid Id { get; set; }
}