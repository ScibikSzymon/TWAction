namespace TWAction.Application.Interfaces;

/// <summary>
/// Provides access to the authenticated user's identity and role.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Tries to read the authenticated user's identifier.
    /// </summary>
    /// <param name="userId">The parsed user identifier when available.</param>
    /// <returns><c>true</c> when a valid user identifier is present; otherwise <c>false</c>.</returns>
    bool TryGetUserId(out Guid userId);

    /// <summary>
    /// Gets a value indicating whether the current user is an administrator.
    /// </summary>
    bool IsAdmin { get; }
}
