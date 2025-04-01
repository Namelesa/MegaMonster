using MegaMonster.Services.Favors.Core.Category;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Core.News;
using MegaMonster.Services.Favors.Core.Ride;
using MegaMonster.Services.Favors.Core.Ticket;
using MegaMonster.Services.Favors.Core.TicketConfiguration;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Favors.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Core.Category.Category> Categories { get; set; }
    public DbSet<Core.Ride.Ride> Rides { get; set; }
    public DbSet<Core.Ticket.Ticket> Tickets { get; set; }
    public DbSet<Core.News.News> News { get; set; }

    public DbSet<Core.TicketConfiguration.TicketConfiguration> Configurations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Core.Category.Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Core.Ride.Ride>()
            .HasIndex(r => r.Name)
            .IsUnique();
        modelBuilder.Entity<Core.TicketConfiguration.TicketConfiguration>()
            .HasIndex(t => t.UserType)
            .IsUnique();
        modelBuilder.Entity<Core.News.News>()
            .HasIndex(n => n.Description)
            .IsUnique();
    }
}