namespace MegaMonster.Services.Notification.Core.Models;

public class BillUserDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    
    public int OrderId { get; set; }
    
    public string PaymentType { get; set; }
    
    public string Status { get; set; }
    public double Sum { get; set; }
    
    public List<int> OrderDetailsRows { get; set; }
}