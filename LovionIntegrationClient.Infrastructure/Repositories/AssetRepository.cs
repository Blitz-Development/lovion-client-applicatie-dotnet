using LovionIntegrationClient.Core.Domain;
using LovionIntegrationClient.Infrastructure.Persistence;

namespace LovionIntegrationClient.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly IntegrationDbContext dbContext;

    public AssetRepository(IntegrationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        // TODO: save to database
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}

