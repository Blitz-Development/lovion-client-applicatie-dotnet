namespace LovionIntegrationClient.Core.Services;

public interface IAssetService
{
    Task ImportFromSoapAsync(CancellationToken cancellationToken = default);
}


