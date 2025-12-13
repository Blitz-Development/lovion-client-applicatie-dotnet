using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Core.Services;

public interface IAssetService
{
    Task ImportFromSoapAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetDto>> GetAllAssetsAsync();

    Task<AssetDto?> GetAssetByIdAsync(Guid id);
}



