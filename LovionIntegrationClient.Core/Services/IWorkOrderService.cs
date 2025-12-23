using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Core.Services;

public interface IWorkOrderService
{
    Task ImportFromSoapAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrderDto>> GetAllWorkOrdersAsync(String? Status = null);

    Task<WorkOrderDto?> GetWorkOrderByIdAsync(Guid id);

    Task<IReadOnlyList<SoapWorkOrderDto>> FetchWorkOrdersFromSoapAsync();
    
    Task MarkAsProcessedAsync(Guid workOrderId);

}



