using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.DependencyInjection;

public static class CareerDevelopmentModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCareerDevelopmentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CareerDevelopmentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CareerDevelopmentDatabase");

            // CareerDevelopment hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil -
            // Employment/Interview ile aynı ADR-012 Sqlite-for-Testing switch'i burada da geçerli.
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
            CareerDevelopmentModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<CareerDevelopmentDbContext>());

        services.AddScoped<ISkillGapRepository, SkillGapRepository>();
        services.AddScoped<ICareerGoalRepository, CareerGoalRepository>();
        services.AddScoped<ITrainingRecommendationRepository, TrainingRecommendationRepository>();
        services.AddScoped<IAdvisorRecommendationRepository, AdvisorRecommendationRepository>();

        return services;
    }
}
