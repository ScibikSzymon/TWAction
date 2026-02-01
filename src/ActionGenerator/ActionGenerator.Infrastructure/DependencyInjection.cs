using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDistanceCalculator, DistanceCalculator>();
        services.AddSingleton<INightTimeChecker, NightTimeChecker>();
        services.AddSingleton<IFrontDistanceCalculator, FrontDistanceCalculator>();

        return services;
    }
}
