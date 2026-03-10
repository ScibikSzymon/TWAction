using Microsoft.Extensions.DependencyInjection;
using TWAction.Domain.Users;

namespace TWAction.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy => policy.RequireRole(nameof(UserRole.Admin)));
            options.AddPolicy(
                AuthorizationPolicies.UserOrAbove,
                policy => policy.RequireRole(nameof(UserRole.User), nameof(UserRole.Admin)));
        });

        return services;
    }
}
