using MegaMonster.Services.Favors.Models;
using MegaMonster.Services.Favors.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MegaMonster.Services.Favors.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Ride>()
            .HasIndex(r => r.Name)
            .IsUnique();
        modelBuilder.Entity<Ticket>()
            .Property(t => t.UserType)
            .HasConversion(new EnumToStringConverter<UserTypeEnum>());
    }
}