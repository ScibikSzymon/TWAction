namespace TWAction.Infrastructure.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TWAction.Application.Interfaces;
using TWAction.Domain.Users;

/// <summary>
/// Reads the current user's identity and role from the HTTP context.
/// </summary>
public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public bool TryGetUserId(out Guid userId)
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var idValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idValue, out userId);
    }

    /// <inheritdoc />
    public bool IsAdmin
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal?.IsInRole(nameof(UserRole.Admin)) ?? false;
        }
    }
}
