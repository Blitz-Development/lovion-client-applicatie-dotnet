using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading.Tasks;

using LovionIntegrationClient.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Soap_Generated;
using CoreSoapWorkOrderDto = LovionIntegrationClient.Core.Dtos.SoapWorkOrderDto;

namespace LovionIntegrationClient.Infrastructure.Soap;

public class SoapWorkOrderClient
{
    private readonly SoapBackendSettings settings;
    private readonly ILogger<SoapWorkOrderClient> _logger;

    public SoapWorkOrderClient(
        IOptions<SoapBackendSettings> options,
        ILogger<SoapWorkOrderClient> logger)
    {
        settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<CoreSoapWorkOrderDto>> GetWorkOrdersAsync()
    {
        // Build endpoint address from BaseUrl (e.g., http://localhost:8080/ws)
        var endpointUri = new Uri(new Uri(settings.BaseUrl ?? "http://localhost:8080"), "/ws");

        // Use the public constructor with endpoint configuration and a custom address
        using var client = new WorkOrdersPortClient(
            WorkOrdersPortClient.EndpointConfiguration.WorkOrdersPortSoap11,
            new EndpointAddress(endpointUri));

        // Create request without status field to get all work orders
        var request = new GetWorkOrdersRequest();

        GetWorkOrdersResponse? response;

        // Retry-policy: 3 pogingen bij transient SOAP-faults of netwerkproblemen
        var retryPolicy = Policy
            .Handle<SoapFaultException>(ex => ex.IsTransient)
            .Or<CommunicationException>()
            .WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8)
                },
                onRetry: (ex, delay, attempt, context) =>
                {
                    _logger.LogWarning(
                        ex,
                        "Retrying SOAP call (attempt {Attempt}) after {Delay}. Reason: {Message}",
                        attempt,
                        delay,
                        ex.Message);
                });

        // Call uitvoeren binnen de policy
        response = await retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                return await client.GetWorkOrdersAsync(request);
            }
            catch (FaultException ex)
            {
                _logger.LogWarning(ex, "WCF FaultException during SOAP call.");

                throw new SoapFaultException(
                    faultCode: ex.Code?.Name,
                    faultString: ex.Message,
                    detail: ex.Reason?.ToString(),
                    isTransient: SoapFaultParser.GuessTransient(
                        ex.Code?.Name,
                        ex.Message,
                        ex.Reason?.ToString()),
                    inner: ex);
            }
            catch (CommunicationException ex)
            {
                _logger.LogWarning(ex, "WCF CommunicationException during SOAP call.");
                throw;
            }
        });

        var items = response?.GetWorkOrdersResponse1 ?? Array.Empty<WorkOrderType>();

        // Als WCF geen items teruggeeft (namespace-gedoe) -> HTTP fallback
        if (items.Length == 0 && response?.Body?.workOrder is null)
        {
            return await FetchWorkOrdersViaHttpFallbackAsync(endpointUri);
        }

        return items
            .Select(w => new CoreSoapWorkOrderDto
            {
                ExternalWorkOrderId = w.externalWorkOrderId,
                ExternalAssetRef    = w.externalAssetRef,
                Description         = w.description,
                ScheduledDate       = w.scheduledDateSpecified ? w.scheduledDate : null,
                WorkType            = w.workType,
                Priority            = w.priority,
                Status              = w.status
            })
            .ToList();
    }

    private async Task<IReadOnlyList<CoreSoapWorkOrderDto>> FetchWorkOrdersViaHttpFallbackAsync(
        Uri endpointUri)
    {
        // Minimal SOAP 1.1 envelope; status-element helemaal weglaten
        var envelope =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:work=\"http://www.loviondummy.nl/workorders\">"
            + "<soapenv:Header/>"
            + "<soapenv:Body>"
            + "<work:GetWorkOrdersRequest/>"
            + "</soapenv:Body>"
            + "</soapenv:Envelope>";

        using var httpClient = new HttpClient();
        using var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "\"\"");

        var response = await httpClient.PostAsync(endpointUri, content);
        var rawXml   = await response.Content.ReadAsStringAsync();

        // 1. Eerst checken of het een SOAP fault is
        ThrowIfSoapFault(rawXml, (int)response.StatusCode, inner: null);

        // 2. Daarna pas gewone HTTP-fouten
        response.EnsureSuccessStatusCode();

        var document   = XDocument.Parse(rawXml);
        var workOrders = document
            .Descendants()
            .Where(e => e.Name.LocalName == "workOrder")
            .Select(wo => new CoreSoapWorkOrderDto
            {
                ExternalWorkOrderId =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "externalWorkOrderId")?.Value,
                ExternalAssetRef =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "externalAssetRef")?.Value,
                Description =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "description")?.Value,
                ScheduledDate =
                    TryParseDateTime(wo.Elements().FirstOrDefault(e => e.Name.LocalName == "scheduledDate")?.Value),
                WorkType =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "workType")?.Value,
                Priority =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "priority")?.Value,
                Status =
                    wo.Elements().FirstOrDefault(e => e.Name.LocalName == "status")?.Value
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

    private void ThrowIfSoapFault(string rawXml, int? httpStatus = null, Exception? inner = null)
    {
        if (SoapFaultParser.TryParse(rawXml, out var fault) && fault is not null)
        {
            var isTransient = SoapFaultParser.GuessTransient(fault.Code, fault.String, fault.Detail);

            _logger.LogWarning(
                "SOAP fault (HTTP fallback). HttpStatus={HttpStatus}. FaultCode={FaultCode}. FaultString={FaultString}. Detail={Detail}. IsTransient={IsTransient}",
                httpStatus,
                fault.Code,
                fault.String,
                fault.Detail,
                isTransient
            );

            throw new SoapFaultException(
                fault.Code,
                fault.String,
                fault.Detail,
                isTransient,
                inner);
        }
    }
}
