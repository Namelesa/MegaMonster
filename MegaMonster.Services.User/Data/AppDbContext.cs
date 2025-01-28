using MegaMonster.Services.User.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Login)
            .IsUnique();
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();
    }
}