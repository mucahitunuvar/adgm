using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Matching.Infrastructure.DependencyInjection;

public static class MatchingModuleServiceCollectionExtensions
{
    public static IServiceCollection AddMatchingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MatchingDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MatchingDatabase");

            // Matching hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil - Employer/
            // Candidate/ReferenceData ile aynı ADR-012 Sqlite-for-Testing switch'i burada da geçerli.
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
            MatchingModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<MatchingDbContext>());

        services.AddScoped<IMatchingCandidateSuggestionRepository, MatchingCandidateSuggestionRepository>();

        return services;
    }
}
