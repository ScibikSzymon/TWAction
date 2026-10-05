using ActionGenerator.MainAction.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.MainAction;

public static class DependencyInjection
{
    public static IServiceCollection AddMainActionGenerator(this IServiceCollection services)
    {
        services.AddScoped<ICommandsStorage, CommandsStorage>();
        services.AddScoped<NobleLimitsChecker>();

        services.AddScoped<ICommandTypeGenerator, NobleGenerator>();
        services.AddScoped<ICommandTypeGenerator, OffGenerator>();
        services.AddScoped<ICommandTypeGenerator, FakeGenerator>();

        services.AddScoped<IActionGenerator, ActionGenerator>();

        return services;
    }
}
