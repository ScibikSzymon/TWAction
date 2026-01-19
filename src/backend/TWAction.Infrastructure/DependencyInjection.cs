using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using TWAction.Application.Interfaces;
using TWAction.Persistence;
using TWAction.Persistence.Repositories;
using TWAction.Application.Handlers;
using TWAction.Application.Users.Queries;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Queries;
using TWAction.Application.Schedules.Commands;
using TWAction.Application.Users.Commands;

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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        services.AddTransient<SignInWithGoogleHandler>();
        services.AddTransient<GetAllUsersHandler>();
        services.AddTransient<GetUserBySessionHandler>();
        services.AddTransient<DeleteSessionHandler>();
        services.AddTransient<GetAllSchedulesHandler>();
        services.AddTransient<GetScheduleByIdHandler>();
        services.AddTransient<CreateScheduleHandler>();
        services.AddTransient<UpdateScheduleHandler>();
        services.AddTransient<DeleteScheduleHandler>();

        return services;
    }
}
