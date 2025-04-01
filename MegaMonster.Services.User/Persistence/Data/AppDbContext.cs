using MegaMonster.Services.User.Core.User;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    
    public DbSet<Users> BannedUsers { get; set; }
    public DbSet<Core.Role.Role> Roles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Login)
            .IsUnique();
        modelBuilder.Entity<Core.Role.Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();
    }
}