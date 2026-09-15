using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Identity;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = $"DataSource=file:{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

    // CAP's UseEntityFramework<T>() only works against SqlServer (see ADR-014's addendum) - it
    // cannot share Identity's Sqlite database, so it gets its own throwaway LocalDB database,
    // uniquely named per test run and dropped in Dispose.
    private readonly string _notificationDatabaseName = $"GenclikMerkezi.Notification.Test.{Guid.NewGuid():N}";

    private readonly SqliteConnection _keepAliveConnection;

    public CustomWebApplicationFactory()
    {
        // A shared-cache Sqlite in-memory database is destroyed once its last connection closes.
        // This connection is kept open for the factory's lifetime so the schema and data created by
        // Program.cs's own (short-lived, per-request) connections survive between HTTP requests.
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    private string NotificationConnectionString =>
        $"Server=(localdb)\\mssqllocaldb;Database={_notificationDatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs reads Jwt/connection-string configuration eagerly (before builder.Build()) while
        // wiring up AddIdentityModule(). ConfigureAppConfiguration() only becomes visible to Program.cs
        // at Build() time, so those eager reads would still see empty values. UseSetting() populates the
        // webhost's settings before the app builder assembles its configuration, so it is visible early.
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:IdentityDatabase", _connectionString);
        builder.UseSetting("Jwt:Issuer", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:Audience", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-do-not-use-in-prod");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");
        builder.UseSetting("ConnectionStrings:NotificationDatabase", NotificationConnectionString);

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
        _keepAliveConnection.Dispose();

        // Best-effort cleanup of the throwaway LocalDB database - EF Core's connection pool may
        // still hold a pooled (but idle) connection open at this point, so this can occasionally
        // fail; that would only leak one small test-run-specific database, not fail the test run.
        try
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(
                "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;TrustServerCertificate=True;");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                $"ALTER DATABASE [{_notificationDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_notificationDatabaseName}];";
            command.ExecuteNonQuery();
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
        }
    }
}
