using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMonster.Services.Favors.Models;

public class Ride(string name) : BaseModel
{
    public string Name { get; set; } = name;

    [Display(Name ="Category")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category CategoryName { get; set; }

    [Required]
    public string ClientStatus { get; set; }

    public double Rating { get; set; }
}