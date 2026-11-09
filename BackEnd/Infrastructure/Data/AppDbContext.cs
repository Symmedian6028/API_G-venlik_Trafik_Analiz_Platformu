using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    { 
    }

    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }
}