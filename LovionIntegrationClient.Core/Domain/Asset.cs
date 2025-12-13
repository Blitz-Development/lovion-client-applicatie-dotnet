using System.ComponentModel.DataAnnotations;

namespace LovionIntegrationClient.Core.Domain;

public class Asset
{
    [Key]    // <-- zegt: dit is de primaire sleutel
    public Guid Id { get; set; }

    public string? ExternalId { get; set; }

    public string? Name { get; set; }
}



