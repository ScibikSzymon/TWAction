namespace TWAction.Api.Options;

using System.ComponentModel.DataAnnotations;

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
    [Required]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Google OAuth client secret.
    /// </summary>
    [Required]
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// Redirect URI configured in Google Console for the OAuth2 callback.
    /// </summary>
    [Required]
    public string RedirectUri { get; set; } = null!;
}
