using MegaMonster.Services.Payment.Core.Payment;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.Payment.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payments> Payments { get; set; }
}