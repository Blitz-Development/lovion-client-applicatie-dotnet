using LovionIntegrationClient.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace LovionIntegrationClient.Infrastructure.Persistence;

public class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();

    public DbSet<ImportError> ImportErrors => Set<ImportError>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ImportError>()
            .Property(e => e.Severity)
            .HasConversion<string>()
            .HasMaxLength(20);
    }


    // TODO: configure database provider
    // TODO: add migrations later
}



