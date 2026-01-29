namespace TWAction.Api.Options;

/// <summary>
/// Configuration options for Google OAuth integration.
/// Bound from the "Google" section in appsettings.
/// </summary>
public sealed class GoogleOptions
{
    public static string SectionName = "Google";

    /// <summary>
    /// Google OAuth client id.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Google OAuth client secret.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Redirect URI configured in Google Console for the OAuth2 callback.
    /// </summary>
    public string? RedirectUri { get; set; }
}
