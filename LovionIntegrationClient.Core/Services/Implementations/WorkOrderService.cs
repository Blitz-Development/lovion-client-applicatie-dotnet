namespace LovionIntegrationClient.Core.Services.Implementations;

public class WorkOrderService : IWorkOrderService
{
    public Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // TODO: call SOAP backend
        // TODO: validate XML
        // TODO: save to database
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}


