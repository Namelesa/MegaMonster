using MegaMonster.Services.Card.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Card.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetails> OrdersDetails { get; set; }
}