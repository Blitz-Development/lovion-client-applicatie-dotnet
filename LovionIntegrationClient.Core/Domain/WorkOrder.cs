namespace LovionIntegrationClient.Core.Domain;

public class WorkOrder
{
    public Guid Id { get; set; }

    public string? ExternalId { get; set; }

    public string? Description { get; set; }

    public DateTime? ScheduledDate { get; set; }
}


