namespace LovionIntegrationClient.Core.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public override string ToString() =>
        IsValid ? (Warnings.Count == 0 ? "Valid" : $"Valid ({Warnings.Count} warnings)")
            : $"Invalid ({Errors.Count} errors, {Warnings.Count} warnings)";
}