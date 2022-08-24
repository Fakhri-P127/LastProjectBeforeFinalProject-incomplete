using LastGrind.Application.Interfaces;
using LastGrind.Persistance.Context;
using LastGrind.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastGrind.Persistance.Services
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddDatabaseConnection(this IServiceCollection services
            ,IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("default"));
            });
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
