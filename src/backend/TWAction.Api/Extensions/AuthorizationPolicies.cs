namespace TWAction.Api.Extensions;

public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy requiring an administrator role.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy requiring a standard user role or higher.
    /// </summary>
    public const string UserOrAbove = "UserOrAbove";
}
