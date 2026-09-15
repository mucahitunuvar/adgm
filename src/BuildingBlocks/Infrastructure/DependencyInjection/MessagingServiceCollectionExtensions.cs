using DotNetCore.CAP;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Messaging;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Savorboard.CAP.InMemoryMessageQueue;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;

public static class MessagingServiceCollectionExtensions
{
    // CAP's UseEntityFramework<T>() is bundled inside its dialect-specific storage package
    // (DotNetCore.CAP.SqlServer here) and is NOT dialect-agnostic - it always issues SqlServer
    // SQL against TDbContext's connection, regardless of what provider TDbContext itself is
    // configured with (verified by running it against a Sqlite-configured DbContext: CAP starts,
    // but its background processors fail on every SQL Server connection-string parse). CAP.SqlServer
    // and CAP.Sqlite both define UseEntityFramework<T>() with the exact same signature too, so
    // referencing both in one project is a compile-time ambiguity on top of that. Consequently,
    // any module that registers messaging must use SqlServer for its DbContext in every
    // environment, including Testing (see ADR-014's addendum) - unlike ADR-012's Sqlite-for-tests
    // switch, which still applies to modules/tests that never touch the outbox.
    //
    // Only the message TRANSPORT varies by environment: "Testing" uses an in-memory queue so
    // integration tests exercise the real publish -> transport -> consume -> side-effect pipeline
    // without a running broker.
    public static IServiceCollection AddMessaging<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        where TDbContext : DbContext
    {
        services.AddCap(x =>
        {
            x.UseEntityFramework<TDbContext>();

            if (environment.IsEnvironment("Testing"))
            {
                x.UseInMemoryMessageQueue();
            }
            else
            {
                x.UseRabbitMQ(options =>
                {
                    options.HostName = configuration["Messaging:RabbitMQ:HostName"] ?? "localhost";
                    options.Port = configuration.GetValue<int?>("Messaging:RabbitMQ:Port") ?? -1;
                    options.UserName = configuration["Messaging:RabbitMQ:UserName"] ?? "guest";
                    options.Password = configuration["Messaging:RabbitMQ:Password"] ?? "guest";
                    options.VirtualHost = configuration["Messaging:RabbitMQ:VirtualHost"] ?? "/";
                });
            }
        });

        services.AddScoped<IIntegrationEventPublisher, CapIntegrationEventPublisher<TDbContext>>();

        return services;
    }
}
