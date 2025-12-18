using System.ServiceModel;
using System.Text;
using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Infrastructure.Configuration;
using Soap_Generated;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Xml.Linq;

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

        // Create request without status field to get all work orders
        // Backend returns 0 results when status is empty string, but returns all when status is null/omitted
        // Using parameterless constructor - status field will be null by default
        // With EmitDefaultValue=false on the DataMember, null should not be serialized
        var request = new GetWorkOrdersRequest();
        
        var response = await client.GetWorkOrdersAsync(request);
        var items = response?.GetWorkOrdersResponse1 ?? Array.Empty<WorkOrderType>();
        
        // WCF deserialization fails because <workOrder> elements are in empty namespace
        // but WCF expects them in http://www.loviondummy.nl/workorders namespace
        // Use HTTP fallback to parse XML directly when WCF returns 0 items
        if (items.Length == 0 && response?.Body?.workOrder is null)
        {
            return await FetchWorkOrdersViaHttpFallbackAsync(endpointUri);
        }

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

    private static async Task<IReadOnlyList<SoapWorkOrderDto>> FetchWorkOrdersViaHttpFallbackAsync(
        Uri endpointUri)
    {
        // Minimal SOAP 1.1 envelope; omit status element entirely
        var envelope =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:work=\"http://www.loviondummy.nl/workorders\">"
            + "<soapenv:Header/>"
            + "<soapenv:Body>"
            + "<work:GetWorkOrdersRequest/>"
            + "</soapenv:Body>"
            + "</soapenv:Envelope>";

        using var httpClient = new HttpClient();
        using var content = new StringContent(envelope, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml"));
        content.Headers.Add("SOAPAction", "\"\"");

        var response = await httpClient.PostAsync(endpointUri, content);
        var rawXml = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var document = XDocument.Parse(rawXml);
        var workOrders = document
            .Descendants()
            .Where(e => e.Name.LocalName == "workOrder")
            .Select(wo => new SoapWorkOrderDto
            {
                ExternalWorkOrderId = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "externalWorkOrderId")?.Value,
                ExternalAssetRef = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "externalAssetRef")?.Value,
                Description = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "description")?.Value,
                ScheduledDate = TryParseDateTime(wo.Elements().FirstOrDefault(e => e.Name.LocalName == "scheduledDate")?.Value),
                WorkType = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "workType")?.Value,
                Priority = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "priority")?.Value,
                Status = wo.Elements().FirstOrDefault(e => e.Name.LocalName == "status")?.Value
            })
            .ToList();

        return workOrders;
    }

    private static DateTime? TryParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, out var dt) ? dt : null;
    }
}



