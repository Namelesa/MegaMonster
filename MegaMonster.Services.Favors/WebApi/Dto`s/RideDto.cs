using System.ComponentModel.DataAnnotations;

namespace MegaMonster.Services.Favors.WebApi.Dto_s;

public class RideDto
{
    [Required] public string CategoryName { get; set; }
    [Required] public string NewName { get; set; }
    [Required] public string Status { get; set; }
    [Required] public double Rating { get; set; }
}