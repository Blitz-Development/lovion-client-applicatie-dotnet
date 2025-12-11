using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;

namespace LovionIntegrationClient.Infrastructure.Services;

public class AssetService : IAssetService
{
    private readonly IntegrationDbContext dbContext;

    public AssetService(IntegrationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // TODO: generate and call SOAP client
        // TODO: validate XML
        // TODO: map and persist assets using dbContext
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}
