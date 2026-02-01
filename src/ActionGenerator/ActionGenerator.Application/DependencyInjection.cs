using ActionGenerator.Application.Common.Interfaces;
using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.ReconnaissanceActions.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IReconnaissanceActionsService>();
        
        services.AddScoped<IReconnaissanceActionsService, ReconnaissanceActionsService>();
        services.AddScoped<IPopulationCalculator, PopulationCalculator>();
        services.AddScoped<ICommandGenerator, CommandGenerator>();

        return services;
    }
}
