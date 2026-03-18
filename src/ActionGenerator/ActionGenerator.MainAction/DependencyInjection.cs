using ActionGenerator.MainAction.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace ActionGenerator.MainAction;

public static class DependencyInjection
{
    public static IServiceCollection AddMainActionGenerator(this IServiceCollection services)
    {
        services.AddSingleton<ICommandTypeGenerator, OffGenerator>();
        services.AddSingleton<ICommandTypeGenerator, FakeGenerator>();

        services.AddSingleton<IActionGenerator, ActionGenerator>();

        return services;
    }
}
