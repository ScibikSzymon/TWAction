using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TWAction.Api.Options;
using TWAction.Infrastructure.Auth;
using TWAction.Infrastructure.Options;

namespace TWAction.Api.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddApiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GoogleOptions>()
            .BindConfiguration(GoogleOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GeneratorApiOptions>()
            .BindConfiguration(GeneratorApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PlemionaRozpiskiApiOptions>()
            .BindConfiguration(PlemionaRozpiskiApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
