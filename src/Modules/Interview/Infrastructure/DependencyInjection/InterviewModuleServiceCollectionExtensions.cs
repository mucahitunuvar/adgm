using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.Modules.Interview.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Infrastructure.DependencyInjection;

public static class InterviewModuleServiceCollectionExtensions
{
    public static IServiceCollection AddInterviewModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InterviewDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("InterviewDatabase");

            // Interview hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil - Employer/
            // Candidate/Matching ile aynı ADR-012 Sqlite-for-Testing switch'i burada da geçerli.
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
            InterviewModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<InterviewDbContext>());

        services.AddScoped<IInterviewRepository, InterviewRepository>();

        return services;
    }
}
