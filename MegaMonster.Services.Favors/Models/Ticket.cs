using MegaMonster.Services.Favors.Models.Enum;

namespace MegaMonster.Services.Favors.Models;

public class Ticket(UserTypeEnum userType) : BaseModel
{
    public string? UserName { get; set; }
    public Guid? UserId { get; set; }
    public UserTypeEnum UserType { get; set; } = userType;
    public double Price { get; set; }
    public DateTime? DateTimeStart { get; set; }
    public DateTime? DateTimeEnd { get; set; }
}