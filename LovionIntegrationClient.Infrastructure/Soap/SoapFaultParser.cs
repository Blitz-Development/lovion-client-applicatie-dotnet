using System;
using System.Linq;
using System.Xml.Linq;

namespace LovionIntegrationClient.Infrastructure.Soap;

public static class SoapFaultParser
{
    public sealed record SoapFaultInfo(string? Code, string? String, string? Detail, bool IsSoap12);

    public static bool TryParse(string xml, out SoapFaultInfo? fault)
    {
        fault = null;
        if (string.IsNullOrWhiteSpace(xml)) return false;

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch
        {
            return false;
        }

        // Zoek <Fault> element (ongeacht namespace)
        var faultElement = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
        if (faultElement is null) return false;

        // SOAP 1.1 typisch:
        // <faultcode>...</faultcode>
        // <faultstring>...</faultstring>
        // <detail>...</detail>

        var faultCode11 = faultElement.Elements().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value?.Trim();
        var faultString11 = faultElement.Elements().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value?.Trim();
        var detail11 = faultElement.Elements().FirstOrDefault(e => e.Name.LocalName == "detail")?.Value?.Trim();

        if (!string.IsNullOrWhiteSpace(faultCode11) || !string.IsNullOrWhiteSpace(faultString11) || !string.IsNullOrWhiteSpace(detail11))
        {
            fault = new SoapFaultInfo(faultCode11, faultString11, detail11, IsSoap12: false);
            return true;
        }

        // SOAP 1.2 typisch:
        // <Code><Value>...</Value></Code>
        // <Reason><Text>...</Text></Reason>
        // <Detail>...</Detail>

        var code12 = faultElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")
                                 ?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Value")
                                 ?.Value?.Trim();

        var reason12 = faultElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "Reason")
                                   ?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Text")
                                   ?.Value?.Trim();

        var detail12 = faultElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "Detail")
                                   ?.Value?.Trim();

        if (!string.IsNullOrWhiteSpace(code12) || !string.IsNullOrWhiteSpace(reason12) || !string.IsNullOrWhiteSpace(detail12))
        {
            fault = new SoapFaultInfo(code12, reason12, detail12, IsSoap12: true);
            return true;
        }

        // Fault gevonden, maar velden onbekend -> alsnog “true” met nulls zou kunnen,
        // maar ik hou ‘m nu “false” om geen onzin te claimen.
        return false;
    }

    public static bool GuessTransient(string? faultCode, string? faultString, string? detail)
    {
        // Simpel heuristiekje voor later. Niet perfect, maar praktisch.
        var text = $"{faultCode} {faultString} {detail}".ToLowerInvariant();

        if (text.Contains("rate limit") || text.Contains("too many requests")) return true;
        if (text.Contains("temporarily") || text.Contains("unavailable") || text.Contains("overloaded")) return true;
        if (text.Contains("timeout")) return true;

        // Auth/credentials is meestal permanent
        if (text.Contains("invalid credential") || text.Contains("authentication")) return false;

        // Default: liever false dan eindeloos retryen
        return false;
    }
}
