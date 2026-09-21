using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.DependencyInjection;

public static class EmployerModuleServiceCollectionExtensions
{
    public static IServiceCollection AddEmployerModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmployerDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("EmployerDatabase");

            // Employer hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil - Candidate/
            // ReferenceData ile aynı ADR-012 Sqlite-for-Testing switch'i burada da geçerli.
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
            EmployerModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<EmployerDbContext>());

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyModuleContract, CompanyModuleContract>();

        return services;
    }
}
