using ActionGenerator.Application.Common.Services;
using ActionGenerator.Application.Features.ReconnaissanceActions.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validators
        services.AddValidatorsFromAssemblyContaining<IReconnaissanceActionsService>();
        
        // Application services (use cases / orchestration)
        services.AddScoped<IReconnaissanceActionsService, ReconnaissanceActionsService>();

        services.AddScoped<INightTimeChecker, NightTimeChecker>();
        services.AddScoped<IFrontDistanceCalculator, FrontDistanceCalculator>();
        services.AddScoped<ICommandGenerator, CommandGenerator>();

        return services;
    }
}

