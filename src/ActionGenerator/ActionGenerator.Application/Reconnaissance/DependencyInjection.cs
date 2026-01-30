using ActionGenerator.Application.Reconnaissance.Commands;
using ActionGenerator.Application.Reconnaissance.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.Application.Reconnaissance;

public static class DependencyInjection
{
    public static IServiceCollection AddReconnaissance(this IServiceCollection services)
    {
        // Register handlers
        services.AddScoped<GenerateReconnaissanceActionsHandler>();

        // Register validators from this vertical slice
        services.AddValidatorsFromAssemblyContaining<GenerateReconnaissanceActionsCommandValidator>(includeInternalTypes: true);

        return services;
    }
}


