using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LovionIntegrationClient.Core.Domain;

public class WorkOrder
{
    [Key]
    public Guid Id { get; set; }

    // Foreign key property naar Asset
    public Guid AssetId { get; set; }

    // Navigatie property: relatie naar Asset
    [ForeignKey("AssetId")]
    public Asset? Asset { get; set; }

    public string? ExternalId { get; set; }

    public string? Description { get; set; }

    public DateTime? ScheduledDate { get; set; }
    
    // Mogelijke statussen:
    // "IMPORTED" – succesvol gevalideerd en binnengehaald
    // "VALIDATION_FAILED" – wel ontvangen maar niet geldig
    // "READY" – klaar voor verwerking
    // "PROCESSED" – volledig verwerkt
    public string Status { get; set; } = "IMPORTED";
}
