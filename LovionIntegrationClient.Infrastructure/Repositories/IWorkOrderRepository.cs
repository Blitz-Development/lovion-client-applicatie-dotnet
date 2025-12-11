using LovionIntegrationClient.Core.Domain;

namespace LovionIntegrationClient.Infrastructure.Repositories;

public interface IWorkOrderRepository
{
    Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default);
}


