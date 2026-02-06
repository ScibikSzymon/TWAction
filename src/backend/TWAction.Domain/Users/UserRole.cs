namespace TWAction.Domain.Users;

/// <summary>
/// Defines the available roles for users in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard user with basic permissions.
    /// </summary>
    User = 0,

    /// <summary>
    /// Administrator with elevated permissions.
    /// </summary>
    Admin = 1
}
