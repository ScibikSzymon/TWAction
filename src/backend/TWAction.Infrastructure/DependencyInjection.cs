using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Wolverine;
using TWAction.Application.Interfaces;
using TWAction.Persistence;
using TWAction.Persistence.Repositories;
using TWAction.Infrastructure.Services;
using TWAction.Infrastructure.Auth;
using TWAction.Infrastructure.Options;
using TWAction.Application.Handlers;
using TWAction.Application.Users.Queries;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Users.Commands;
using TWAction.Application.Schedules.Services;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Tribes.Queries;
using TWAction.Application.Settings.Interfaces;
using TWAction.Application.Settings.Queries;
using TWAction.Application.Settings.Commands;
using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.AttackCommands.Handlers;
using TWAction.Application.ReconnaissanceActions.Interfaces;
using TWAction.Application.ReconnaissanceActions.Handlers;


namespace TWAction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("TWActionDatabase");
        services.AddDbContext<TWActionDbContext>(opts => opts.UseNpgsql(conn));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        // Right now this keeps the default in-process bus (no external transports) 
        services.AddWolverine(opts =>
        {
            opts.Durability.Mode = DurabilityMode.MediatorOnly;
            opts.Discovery.IncludeAssembly(typeof(SignInWithGoogleHandler).Assembly);
        });

        // Register HttpClient factory and IMemoryCache for TribalWars Api calls
        services.AddHttpClient<TribesHttpService>();
        services.AddMemoryCache();

        // Register Generator.Api HTTP client
        services.AddHttpClient<IGeneratorApiClient, GeneratorApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GeneratorApiOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
            client.DefaultRequestHeaders.Add("X-Api-KEY", options.Value.ApiKey);
        });

        // Register PlemionaRozpiski.pl API client
        services.AddHttpClient<IPlemionaRozpiskiApiClient, PlemionaRozpiskiApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PlemionaRozpiskiApiOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
            client.DefaultRequestHeaders.Add("X-API-KEY", options.Value.ApiKey);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ITroopsStateRepository, TroopsStateRepository>();
        services.AddScoped<IReconnaissanceSettingsRepository, ReconnaissanceSettingsRepository>();
        services.AddScoped<IAttackCommandRepository, AttackCommandRepository>();


        services.AddSingleton<TroopsStateValidator>();
        services.AddSingleton<TroopsStateCompressionService>();
        services.AddSingleton<TroopsStateStatsExtractor>();
        services.AddSingleton<TribesCsvParser>();
        services.AddSingleton<PlayersCsvParser>();
        services.AddSingleton<VillagesCsvParser>();
        services.AddScoped<ITribesService, TribesHttpService>();

        services.AddTransient<SignInWithGoogleHandler>();
        services.AddTransient<GetAllUsersHandler>();
        services.AddTransient<GetUserBySessionHandler>();
        services.AddTransient<DeleteSessionHandler>();
        services.AddTransient<GetAllSchedulesHandler>();
        services.AddTransient<GetScheduleByIdHandler>();
        services.AddTransient<CreateScheduleHandler>();
        services.AddTransient<UpdateScheduleHandler>();
        services.AddTransient<DeleteScheduleHandler>();
        services.AddTransient<UploadTroopsStateHandler>();
        services.AddTransient<GetTroopsStateHandler>();
        services.AddTransient<GetTribesHandler>();
        services.AddTransient<GetReconnaissanceSettingsHandler>();
        services.AddTransient<SaveReconnaissanceSettingsHandler>();
        services.AddTransient<GenerateReconnaissanceActionsHandler>();
        services.AddTransient<GetAttackCommandsSummaryHandler>();
        services.AddTransient<SendToPlemionaRozpiskiHandler>();

        // Register authentication with session-based authentication handler
        services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
                SessionAuthenticationHandler.SchemeName, 
                options => { });

        return services;
    }
}

