using MegaMonster.Services.Favors.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
}