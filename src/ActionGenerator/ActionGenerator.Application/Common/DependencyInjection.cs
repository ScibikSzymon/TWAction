using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        // Register common application services here
        return services;
    }
}
