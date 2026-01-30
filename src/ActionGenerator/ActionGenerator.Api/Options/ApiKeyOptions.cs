namespace ActionGenerator.Api.Options;

public sealed class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-API-Key";
    
    public IReadOnlyList<string> ValidKeys { get; set; } = Array.Empty<string>();
}
