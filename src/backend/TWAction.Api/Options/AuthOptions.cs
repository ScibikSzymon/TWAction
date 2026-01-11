namespace TWAction.Api.Options;

/// <summary>
/// Configuration options for authentication settings.
/// Bound from the "Auth" section in appsettings.
/// </summary>
public sealed class AuthOptions
{
    /// <summary>
    /// Name of the cookie used to store the application session id.
    /// </summary>
    public string? CookieName { get; set; }

    /// <summary>
    /// Number of hours before the session expires.
    /// </summary>
    public int SessionExpiryHours { get; set; }

    /// <summary>
    /// Indicates whether the cookie should only be transmitted over HTTPS.
    /// </summary>
    public bool CookieSecure { get; set; }

    /// <summary>
    /// Defines the same-site policy for the cookie (e.g., "Strict", "Lax", "None").
    /// </summary>
    public string? CookieSameSite { get; set; }

    /// <summary>
    /// Domain for which the cookie is valid.
    /// </summary>
    public string? CookieDomain { get; set; }
}
