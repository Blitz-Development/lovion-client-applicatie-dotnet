namespace LovionIntegrationClient.Core.Domain;

public class ImportError
{
    public Guid Id { get; set; }

    public Guid? ImportRunId { get; set; }

    public string? Message { get; set; }

    public string? PayloadReference { get; set; }
}



