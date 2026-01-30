using ActionGenerator.Application.Common;
using ActionGenerator.Application.Reconnaissance;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register vertical slices
        services.AddCommon();
        services.AddReconnaissance();
        
        // Future vertical slices:
        // services.AddFake();
        // services.AddMain();

        return services;
    }
}

