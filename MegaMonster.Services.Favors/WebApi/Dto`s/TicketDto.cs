namespace MegaMonster.Services.Favors.WebApi.Dto_s;

public class TicketDto
{
    public string UserType { get; set; }
    public string UserName { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateTimeStart { get; set; }
}