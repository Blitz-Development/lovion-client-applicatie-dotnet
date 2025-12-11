namespace LovionIntegrationClient.Core.Domain;

public class ImportRun
{
    public Guid Id { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? SourceSystem { get; set; }
}



