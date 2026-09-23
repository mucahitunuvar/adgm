using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Infrastructure.Jobs;
using GenclikMerkezi.Modules.Support.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Infrastructure.DependencyInjection;

public static class SupportModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSupportModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SupportDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("SupportDatabase");

            // Support hiçbir mesaj yayınlamaz, CAP transactional outbox anchor'ı değil - Employment
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
            SupportModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<SupportDbContext>());

        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ISupportTicketMessageRepository, SupportTicketMessageRepository>();

        // CloseOverdueSupportTicketsJob yalnızca singleton-güvenli bağımlılıklar alır
        // (IServiceScopeFactory/IConfiguration) ve scoped servisleri kendi içinde açtığı scope'tan
        // çözer (Hangfire job'ları HTTP request scope'unda çalışmaz) - bu yüzden kendisi de Scoped
        // yerine burada Transient kaydedilir, Program.cs'teki RecurringJob.AddOrUpdate<T>() bunu çözer.
        services.AddTransient<CloseOverdueSupportTicketsJob>();

        return services;
    }
}
