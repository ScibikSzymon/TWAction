namespace TWAction.Api.Options;

/// <summary>
/// Configuration options for Cross-Origin Resource Sharing (CORS).
/// </summary>
public sealed class CorsOptions
{
    public static string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS requests.
    /// Must be exact URLs including protocol and port (e.g., "https://yourdomain.com").
    /// Required when using credentials (cookies) in cross-origin requests.
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];
}