namespace LovionIntegrationClient.Infrastructure.Xml;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; } = new();

    public override string ToString() =>
        IsValid ? "Valid" : $"Invalid ({Errors.Count} errors)";
}