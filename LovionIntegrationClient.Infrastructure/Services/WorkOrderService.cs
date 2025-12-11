using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;

namespace LovionIntegrationClient.Infrastructure.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IntegrationDbContext dbContext;

    public WorkOrderService(IntegrationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // TODO: call SOAP backend
        // TODO: validate XML
        // TODO: map and persist work orders using dbContext
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}
