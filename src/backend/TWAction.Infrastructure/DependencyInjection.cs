using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using TWAction.Application.Interfaces;
using TWAction.Persistence;
using TWAction.Persistence.Repositories;

namespace TWAction.Infrastructure
{
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
                
            });

            return services;
        }
    }
}
