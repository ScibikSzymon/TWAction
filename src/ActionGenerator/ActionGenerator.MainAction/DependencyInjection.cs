using ActionGenerator.MainAction.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.MainAction;

public static class DependencyInjection
{
    public static IServiceCollection AddMainActionGenerator(this IServiceCollection services)
    {
        // Order matters: noble sources are excluded from Off/Fake generation via alreadyGenerated
        services.AddSingleton<ICommandTypeGenerator, NobleGenerator>();
        services.AddSingleton<ICommandTypeGenerator, OffGenerator>();
        services.AddSingleton<ICommandTypeGenerator, FakeGenerator>();

        services.AddSingleton<IActionGenerator, ActionGenerator>();

        return services;
    }
}
