using System.ComponentModel.DataAnnotations;

namespace LovionIntegrationClient.Core.Domain;

public class ImportRun
{
    [Key]
    public Guid Id { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
    
    public string Status { get; set; } = default!; 
    // bijv. "RUNNING", "SUCCESS", "PARTIAL_SUCCESS", "FAILED"

    public string SourceSystem { get; set; } = default!;
    // bijv. "DummyLovionSoapBackend"
    
    // Navigatie naar fouten (optioneel maar handig)
    public ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
}



