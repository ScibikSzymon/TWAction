using Microsoft.AspNetCore.Authentication;

namespace ActionGenerator.Api.Security;

/// <summary>
/// Provides configuration for API key authentication.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the configured API key value.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
