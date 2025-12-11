namespace LovionIntegrationClient.Core.Services.Implementations;

public class AssetService : IAssetService
{
    public Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // TODO: generate and call SOAP client
        // TODO: validate XML
        // TODO: save to database
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}


