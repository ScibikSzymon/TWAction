namespace ActionGenerator.Api.Options;

public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
