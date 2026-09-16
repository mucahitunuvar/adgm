using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Identity;

// Both databases are real (throwaway, per-test-run) LocalDB databases rather than ADR-012's usual
// Sqlite switch: IdentityDbContext is the CAP transactional outbox anchor (Program.cs's
// AddMessaging<IdentityDbContext>() call - CAP only supports one instance per process, see
// ADR-014's amendment), and CAP's SqlServer storage package cannot target a Sqlite connection.
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string LocalDbServer = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _identityDatabaseName = $"GenclikMerkezi.Identity.Test.{Guid.NewGuid():N}";
    private readonly string _notificationDatabaseName = $"GenclikMerkezi.Notification.Test.{Guid.NewGuid():N}";

    private string IdentityConnectionString =>
        $"{LocalDbServer}Database={_identityDatabaseName};";

    private string NotificationConnectionString =>
        $"{LocalDbServer}Database={_notificationDatabaseName};";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs reads Jwt/connection-string configuration eagerly (before builder.Build()) while
        // wiring up AddIdentityModule(). ConfigureAppConfiguration() only becomes visible to Program.cs
        // at Build() time, so those eager reads would still see empty values. UseSetting() populates the
        // webhost's settings before the app builder assembles its configuration, so it is visible early.
        builder.UseSetting("ConnectionStrings:IdentityDatabase", IdentityConnectionString);
        builder.UseSetting("ConnectionStrings:NotificationDatabase", NotificationConnectionString);
        builder.UseSetting("Jwt:Issuer", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:Audience", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-do-not-use-in-prod");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DropDatabase(_identityDatabaseName);
        DropDatabase(_notificationDatabaseName);
    }

    // Best-effort cleanup of the throwaway LocalDB databases - EF Core's connection pool may
    // still hold a pooled (but idle) connection open at this point, so this can occasionally
    // fail; that would only leak one small test-run-specific database, not fail the test run.
    private static void DropDatabase(string databaseName)
    {
        try
        {
            using var connection = new SqlConnection(LocalDbServer);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];";
            command.ExecuteNonQuery();
        }
        catch (SqlException)
        {
        }
    }
}
