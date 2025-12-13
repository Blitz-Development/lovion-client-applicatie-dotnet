using System.Xml.Linq;
using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Infrastructure.Xml;

public class XmlWorkOrderSerializer
{
    private static readonly XNamespace Ns = "http://www.loviondummy.nl/workorders";

    public string ToXml(SoapWorkOrderDto dto)
    {
        var root = new XElement(Ns + "workOrder");

        // verplichte + optionele velden, alleen element maken als er een waarde is
        if (!string.IsNullOrWhiteSpace(dto.ExternalWorkOrderId))
            root.Add(new XElement(Ns + "externalWorkOrderId", dto.ExternalWorkOrderId));

        if (!string.IsNullOrWhiteSpace(dto.ExternalAssetRef))
            root.Add(new XElement(Ns + "externalAssetRef", dto.ExternalAssetRef));

        if (!string.IsNullOrWhiteSpace(dto.Description))
            root.Add(new XElement(Ns + "description", dto.Description));

        if (dto.ScheduledDate.HasValue)
            root.Add(new XElement(Ns + "scheduledDate", dto.ScheduledDate.Value));

        if (!string.IsNullOrWhiteSpace(dto.WorkType))
            root.Add(new XElement(Ns + "workType", dto.WorkType));

        if (!string.IsNullOrWhiteSpace(dto.Priority))
            root.Add(new XElement(Ns + "priority", dto.Priority));

        if (!string.IsNullOrWhiteSpace(dto.Status))
            root.Add(new XElement(Ns + "status", dto.Status));

        return root.ToString(SaveOptions.DisableFormatting);
    }
}