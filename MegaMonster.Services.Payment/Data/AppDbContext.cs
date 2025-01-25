using MegaMonster.Services.Payment.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Payment.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payments> Payments { get; set; }
}