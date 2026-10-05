using System.Security.Claims;

namespace TWAction.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the authenticated user's ID from the ClaimsPrincipal.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the authenticated user.</param>
    /// <returns>The user's ID as a Guid, or null if not found or invalid.</returns>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Checks if the authenticated user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }
}
