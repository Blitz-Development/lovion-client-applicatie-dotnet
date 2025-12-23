using System;

namespace LovionIntegrationClient.Infrastructure.Soap;

public sealed class SoapFaultException : Exception
{
    public string? FaultCode { get; }
    public string? FaultString { get; }
    public string? Detail { get; }

    // Handig voor later (retry): is dit waarschijnlijk tijdelijk?
    public bool IsTransient { get; }

    public SoapFaultException(
        string? faultCode,
        string? faultString,
        string? detail,
        bool isTransient,
        Exception? inner = null)
        : base(BuildMessage(faultCode, faultString, detail), inner)
    {
        FaultCode = faultCode;
        FaultString = faultString;
        Detail = detail;
        IsTransient = isTransient;
    }

    private static string BuildMessage(string? code, string? faultString, string? detail)
    {
        // Korte, logvriendelijke message
        return $"SOAP Fault: Code={code ?? "(null)"}, String={faultString ?? "(null)"}, Detail={detail ?? "(null)"}";
    }
}