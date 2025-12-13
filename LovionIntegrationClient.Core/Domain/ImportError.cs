using System.ComponentModel.DataAnnotations;

namespace LovionIntegrationClient.Core.Domain;

public class ImportError
{
    [Key]
    public Guid Id { get; set; }

    // Foreign key naar ImportRun
    public Guid ImportRunId { get; set; }
    public ImportRun ImportRun { get; set; } = default!;
    
    public string? ExternalWorkOrderId { get; set; }

    public string? Message { get; set; }

    public string? PayloadReference { get; set; }
    
    public string ErrorType { get; set; } = default!;
    // bijv. "VALIDATION", "MAPPING", "SOAP_FAULT", ...
    
    public string ErrorMessage { get; set; } = default!;
}



