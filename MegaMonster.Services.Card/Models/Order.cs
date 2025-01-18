using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMonster.Services.Card.Models;

public class Order : BaseModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    
    [Display(Name ="OrderDetail")]
    public int OrderDetailId { get; set; }

    [ForeignKey("OrderDetailId")]
    public OrderDetails OrderDetails { get; set; }

    public double Sum { get; set; }
}