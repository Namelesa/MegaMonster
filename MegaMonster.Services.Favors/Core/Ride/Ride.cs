using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.Favors.Core.Models;

namespace MegaMonster.Services.Favors.Core.Ride;

public class Ride(string name) : BaseModel
{
    public string Name { get; set; } = name;
    
    public int CategoryId { get; set; }
    
    [Required]
    public string ClientStatus { get; set; }

    public double Rating { get; set; }
}