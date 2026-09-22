using GenclikMerkezi.Modules.Employment.Application.Abstractions;
using GenclikMerkezi.Modules.Employment.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employment.Infrastructure.DependencyInjection;

public static class EmploymentModuleServiceCollectionExtensions
{
    public static IServiceCollection AddEmploymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmploymentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("EmploymentDatabase");

            // Employment hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil - Interview
            // ile aynı ADR-012 Sqlite-for-Testing switch'i burada da geçerli.
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
            EmploymentModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<EmploymentDbContext>());

        services.AddScoped<IEmploymentRepository, EmploymentRepository>();
        services.AddScoped<IEmploymentNoteRepository, EmploymentNoteRepository>();

        return services;
    }
}
