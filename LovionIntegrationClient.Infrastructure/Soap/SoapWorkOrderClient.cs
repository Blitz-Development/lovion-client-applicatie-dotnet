using System.ServiceModel;
using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Infrastructure.Configuration;
using LovionIntegrationClient.Infrastructure.Soap.Generated;
using Microsoft.Extensions.Options;

namespace LovionIntegrationClient.Infrastructure.Soap;

public class SoapWorkOrderClient
{
    private readonly SoapBackendSettings settings;

    public SoapWorkOrderClient(IOptions<SoapBackendSettings> options)
    {
        settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<IReadOnlyList<SoapWorkOrderDto>> GetWorkOrdersAsync()
    {
        // Build endpoint address from BaseUrl (e.g., http://localhost:8080/ws)
        var endpointUri = new Uri(new Uri(settings.BaseUrl ?? "http://localhost:8080"), "/ws");

        // Use the public constructor with endpoint configuration and a custom address
        using var client = new WorkOrdersPortClient(
            WorkOrdersPortClient.EndpointConfiguration.WorkOrdersPortSoap11,
            new EndpointAddress(endpointUri));

        var response = await client.GetWorkOrdersAsync(new GetWorkOrdersRequest());
        var items = response?.GetWorkOrdersResponse1 ?? Array.Empty<WorkOrderType>();

        return items
            .Select(w => new SoapWorkOrderDto
            {
                ExternalWorkOrderId = w.externalWorkOrderId,
                ExternalAssetRef = w.externalAssetRef,
                Description = w.description,
                ScheduledDate = w.scheduledDateSpecified ? w.scheduledDate : null,
                WorkType = w.workType,
                Priority = w.priority,
                Status = w.status
            })
            .ToList();
    }
}



