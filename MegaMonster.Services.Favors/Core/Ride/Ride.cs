using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.Core.Ride;

public class Ride(string name) : BaseModel.BaseModel
{
    public string Name { get; set; } = name;
    
    public int CategoryId { get; set; }
    
    [Required]
    public string ClientStatus { get; set; }

    public double Rating { get; set; }
}