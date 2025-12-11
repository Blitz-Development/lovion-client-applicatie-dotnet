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

    // TODO: configure database provider
    // TODO: add migrations later
}



