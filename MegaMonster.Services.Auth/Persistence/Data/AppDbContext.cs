using MegaMonster.Services.Auth.Core.User;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Auth.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Login)
            .IsUnique();
    }
}