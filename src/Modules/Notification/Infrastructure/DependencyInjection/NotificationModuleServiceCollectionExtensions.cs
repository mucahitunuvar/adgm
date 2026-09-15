using GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Notification;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.DependencyInjection;

public static class NotificationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<NotificationDbContext>(options =>
        {
            // Unlike Identity, this is not config-switched to Sqlite in Testing: CAP's
            // UseEntityFramework<T>() (see AddMessaging below) always issues SqlServer SQL
            // against this DbContext's connection, so it must be SqlServer in every
            // environment for the outbox to work at all (ADR-014 addendum).
            var connectionString = configuration.GetConnectionString("NotificationDatabase");
            options.UseSqlServer(connectionString);
        });

        services.AddKeyedScoped<SharedKernel.Abstractions.IUnitOfWork>(
            NotificationModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();

        services.AddMessaging<NotificationDbContext>(configuration, environment);

        return services;
    }
}
