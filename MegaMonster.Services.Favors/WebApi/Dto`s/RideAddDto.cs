using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.WebApi.Dto_s;

public class RideAddDto
{
    [Required] public string CategoryName { get; set; }
    [Required] public string RideName { get; set; }
    [Required] public string Status { get; set; }
    [Required] public double Rating { get; set; }
}