using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Application.Abstractions;
using GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.DependencyInjection;

public static class CareerAdvisorModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCareerAdvisorModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CareerAdvisorDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CareerAdvisorDatabase");

            if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddKeyedScoped<IUnitOfWork>(
            CareerAdvisorModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<CareerAdvisorDbContext>());

        services.AddScoped<ICareerAdvisorRepository, CareerAdvisorRepository>();
        services.AddScoped<ICareerAdvisorModuleContract, CareerAdvisorModuleContract>();

        return services;
    }
}
