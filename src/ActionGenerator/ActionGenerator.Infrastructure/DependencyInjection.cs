using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Technical services (pure calculations, no business logic)
        services.AddSingleton<INightTimeChecker, NightTimeChecker>();
        services.AddSingleton<IFrontDistanceCalculator, FrontDistanceCalculator>();
        services.AddSingleton<ICommandGenerator, CommandGenerator>();
        services.AddSingleton<IPopulationCalculator, PopulationCalculator>();

        return services;
    }
}

