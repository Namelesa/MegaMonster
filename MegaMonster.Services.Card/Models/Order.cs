using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMonster.Services.Card.Models;

public class Order : BaseModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public double Sum { get; set; }
    
    public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
}
