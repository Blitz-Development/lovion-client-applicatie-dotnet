namespace LovionIntegrationClient.Core.Dtos;

public class AssetDto
{
    public Guid Id { get; set; }
    public string? ExternalAssetRef { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
}