using LovionIntegrationClient.Core.Domain;
using LovionIntegrationClient.Infrastructure.Persistence;

namespace LovionIntegrationClient.Infrastructure.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly IntegrationDbContext dbContext;

    public WorkOrderRepository(IntegrationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        // TODO: save to database
        // TODO: add logging here later.
        return Task.CompletedTask;
    }
}


