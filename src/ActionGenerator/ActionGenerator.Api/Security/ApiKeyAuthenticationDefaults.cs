namespace ActionGenerator.Api;

/// <summary>
/// Defines constants for API key authentication.
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    /// <summary>
    /// The authentication scheme name.
    /// </summary>
    public const string Scheme = "ApiKey";

    /// <summary>
    /// The header name used to pass the API key.
    /// </summary>
    public const string HeaderName = "X-Api-KEY";
}
