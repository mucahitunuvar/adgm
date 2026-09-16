using GenclikMerkezi.Modules.Notification;
using GenclikMerkezi.Modules.Notification.Application;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Features.SendPasswordResetEmail;
using GenclikMerkezi.Modules.Notification.Features.SendVerificationEmail;
using GenclikMerkezi.Modules.Notification.Infrastructure.Email;
using GenclikMerkezi.Modules.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.DependencyInjection;

public static class NotificationModuleServiceCollectionExtensions
{
    // Does not register messaging (CAP) itself - CAP only supports a single instance per
    // process (see ADR-014's amendment), so it is registered exactly once at the host
    // composition root, anchored to whichever module currently publishes. Notification's
    // [CapSubscribe] consumers still get discovered by that one shared registration
    // regardless of which module/assembly they live in.
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("NotificationDatabase");

            // "Database:Provider" mirrors Identity's ADR-012 switch: Sqlite only in the
            // Testing environment, SqlServer everywhere else. Unlike Identity, Notification's
            // own DbContext is never the CAP transactional anchor, so this is unconstrained.
            if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddKeyedScoped<SharedKernel.Abstractions.IUnitOfWork>(
            NotificationModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>();

        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.Configure<AppLinkSettings>(configuration.GetSection(AppLinkSettings.SectionName));

        // Registered so CAP (scanning the DI container for ICapSubscribe implementations) can
        // discover them; never resolved directly by application code.
        services.AddTransient<UserRegisteredIntegrationEventConsumer>();
        services.AddTransient<PasswordResetRequestedIntegrationEventConsumer>();

        return services;
    }
}
