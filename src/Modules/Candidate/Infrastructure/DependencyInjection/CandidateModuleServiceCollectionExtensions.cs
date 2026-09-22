using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Infrastructure.Pdf;
using GenclikMerkezi.Modules.Candidate.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.DependencyInjection;

public static class CandidateModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCandidateModule(this IServiceCollection services, IConfiguration configuration)
    {
        // QuestPDF requires its license to be set once, process-wide, before any document is
        // generated (ADR-021: Community License - the project's annual revenue is under $1M and it
        // is not a public company, so no paid tier is required).
        QuestPDF.Settings.License = LicenseType.Community;

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
        services.AddScoped<ICandidateCvPdfExportService, QuestPdfCandidateCvExportService>();
        services.AddScoped<ICandidateModuleContract, CandidateModuleContract>();

        return services;
    }
}
