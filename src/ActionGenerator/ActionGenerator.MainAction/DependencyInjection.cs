using ActionGenerator.MainAction.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.MainAction;

public static class DependencyInjection
{
    public static IServiceCollection AddMainActionGenerator(this IServiceCollection services)
    {
        services.AddScoped<ICommandsStorage, CommandsStorage>();

        // Order matters: noble sources are excluded from Off/Fake generation via storage
        services.AddScoped<ICommandTypeGenerator, NobleGenerator>();
        services.AddScoped<ICommandTypeGenerator, OffGenerator>();
        services.AddScoped<ICommandTypeGenerator, FakeGenerator>();

        services.AddScoped<IActionGenerator, ActionGenerator>();

        return services;
    }
}
