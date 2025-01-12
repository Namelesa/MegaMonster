using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMonster.Services.Favors.Models;

public class Ride : BaseModel
{
    public string Name { get; set; }

    [Display(Name ="Category")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category CategoryName { get; set; }
}