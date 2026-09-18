using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.DependencyInjection;

public static class CandidateModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCandidateModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CandidateDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CandidateDatabase");

            // Candidate never publishes messages, so unlike Identity it is not the CAP transactional
            // outbox anchor and has no constraint tying it to SqlServer in every environment -
            // ADR-012's usual Sqlite-for-Testing switch applies here (same as ReferenceData).
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
            CandidateModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<CandidateDbContext>());

        services.AddScoped<ICandidateCvRepository, CandidateCvRepository>();
        services.AddScoped<ICandidateCvContentRepository, CandidateCvContentRepository>();
        services.AddScoped<ICandidateSearchIndexRepository, CandidateSearchIndexRepository>();

        return services;
    }
}
