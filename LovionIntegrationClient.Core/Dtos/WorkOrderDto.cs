namespace LovionIntegrationClient.Core.Dtos;

public class WorkOrderDto
{
    public Guid Id { get; set; } 
    public string? ExternalWorkOrderId { get; set; }
    public string? WorkType { get; set; }
    public string? Priority { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Status { get; set; }
    public Guid? AssetId { get; set; }
}
