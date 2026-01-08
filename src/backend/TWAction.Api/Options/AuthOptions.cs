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
}
