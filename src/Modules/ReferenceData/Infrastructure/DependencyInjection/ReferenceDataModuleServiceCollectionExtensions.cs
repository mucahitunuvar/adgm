using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.ReferenceData;

public static class ReferenceDataModuleServiceCollectionExtensions
{
    public static IServiceCollection AddReferenceDataModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReferenceDataDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ReferenceDataDatabase");

            // ADR-012's usual Sqlite-for-Testing switch applies here (unlike Identity):
            // ReferenceData never publishes messages, so it is never the CAP transactional outbox
            // anchor and has no constraint tying it to SqlServer in every environment.
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
            ReferenceDataModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<ReferenceDataDbContext>());

        services.AddMemoryCache();
        services.AddScoped<IReferenceDataLookupReader, ReferenceDataLookupReader>();

        return services;
    }
}
