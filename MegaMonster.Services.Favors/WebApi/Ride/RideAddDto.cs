using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.WebApi.Ride;

public class RideAddDto
{
    [Required] public string CategoryName { get; set; }
    [Required] public string RideName { get; set; }
    [Required] public string Status { get; set; }
    [Required] public double Rating { get; set; }
    [Required] public string Image { get; set; }
}