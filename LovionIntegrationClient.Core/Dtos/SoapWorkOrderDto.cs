namespace LovionIntegrationClient.Core.Dtos;

public class SoapWorkOrderDto
{
    public string? ExternalWorkOrderId { get; set; }
    public string? ExternalAssetRef { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? WorkType { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
}

