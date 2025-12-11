using LovionIntegrationClient.Core.Domain;

namespace LovionIntegrationClient.Infrastructure.Repositories;

public interface IAssetRepository
{
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
}



