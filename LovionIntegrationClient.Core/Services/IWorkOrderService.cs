namespace LovionIntegrationClient.Core.Services;

public interface IWorkOrderService
{
    Task ImportFromSoapAsync(CancellationToken cancellationToken = default);
}

