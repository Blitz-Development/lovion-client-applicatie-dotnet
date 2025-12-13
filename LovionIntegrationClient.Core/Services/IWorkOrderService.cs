using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Core.Services;

public interface IWorkOrderService
{
    Task ImportFromSoapAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrderDto>> GetAllWorkOrdersAsync();

    Task<WorkOrderDto?> GetWorkOrderByIdAsync(Guid id);

    Task<IReadOnlyList<SoapWorkOrderDto>> FetchWorkOrdersFromSoapAsync();
}



