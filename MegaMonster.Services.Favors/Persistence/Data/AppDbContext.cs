using MegaMonster.Services.Favors.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<News> News { get; set; }

    public DbSet<TicketConfiguration> Configurations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Ride>()
            .HasIndex(r => r.Name)
            .IsUnique();
        modelBuilder.Entity<TicketConfiguration>()
            .HasIndex(t => t.UserType)
            .IsUnique();
        modelBuilder.Entity<News>()
            .HasIndex(n => n.Description)
            .IsUnique();
    }
}