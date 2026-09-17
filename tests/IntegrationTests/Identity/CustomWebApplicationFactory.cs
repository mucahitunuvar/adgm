using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Identity;

// All three databases are real (throwaway, per-test-run) LocalDB databases rather than ADR-012's
// usual Sqlite switch: IdentityDbContext is the CAP transactional outbox anchor (Program.cs's
// AddMessaging<IdentityDbContext>() call - CAP only supports one instance per process, see
// ADR-014's amendment), and CAP's SqlServer storage package cannot target a Sqlite connection.
// ReferenceData has no such constraint (ADR-012's Sqlite switch would work for it) but uses
// LocalDB too here, simply for consistency with the other two in this shared test factory.
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string LocalDbServer = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _identityDatabaseName = $"GenclikMerkezi.Identity.Test.{Guid.NewGuid():N}";
    private readonly string _notificationDatabaseName = $"GenclikMerkezi.Notification.Test.{Guid.NewGuid():N}";
    private readonly string _referenceDataDatabaseName = $"GenclikMerkezi.ReferenceData.Test.{Guid.NewGuid():N}";

    private string IdentityConnectionString =>
        $"{LocalDbServer}Database={_identityDatabaseName};";

    private string NotificationConnectionString =>
        $"{LocalDbServer}Database={_notificationDatabaseName};";

    private string ReferenceDataConnectionString =>
        $"{LocalDbServer}Database={_referenceDataDatabaseName};";

    public FakeEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs reads Jwt/connection-string configuration eagerly (before builder.Build()) while
        // wiring up AddIdentityModule(). ConfigureAppConfiguration() only becomes visible to Program.cs
        // at Build() time, so those eager reads would still see empty values. UseSetting() populates the
        // webhost's settings before the app builder assembles its configuration, so it is visible early.
        builder.UseSetting("ConnectionStrings:IdentityDatabase", IdentityConnectionString);
        builder.UseSetting("ConnectionStrings:NotificationDatabase", NotificationConnectionString);
        builder.UseSetting("ConnectionStrings:ReferenceDataDatabase", ReferenceDataConnectionString);
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
            scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>().Database.EnsureCreated();
        });

        // Runs after Program.cs's own AddNotificationModule() registration, so this replaces the
        // real SmtpEmailSender - no real mail server is involved in these tests.
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    // Admin cannot be created through the public register endpoint (RegisterUserCommandValidator
    // only allows Candidate/Employer to self-register), so admin-only endpoint tests seed one
    // directly against the same DbContext/repository the app itself uses. Email is pre-confirmed
    // since that is orthogonal to what these tests exercise.
    public async Task<Guid> SeedAdminUserAsync(string email, string password)
    {
        using var scope = Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(IdentityModuleMarker.UnitOfWorkKey);

        var user = User.Register(
            Email.Create(email).Value,
            PasswordHash.FromHashedValue(passwordHasher.Hash(password)),
            UserRole.Admin);

        var verificationTokenHash = $"seed-admin-verification-hash-{Guid.NewGuid():N}";
        user.IssueEmailVerificationToken(verificationTokenHash, DateTime.UtcNow.AddDays(1));
        user.ConfirmEmail(verificationTokenHash);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync();

        return user.Id;
    }

    // No query endpoint reads this back on its own merits yet at the time some tests need it
    // (AdminAuditLog rows are written by domain event handlers, asynchronously to the admin
    // command's own SaveChangesAsync) - reads directly against the same DbContext the app uses.
    public async Task<List<AdminAuditLogEntry>> GetAdminAuditLogEntriesAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        return await dbContext.AdminAuditLogEntries.AsNoTracking().ToListAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DropDatabase(_identityDatabaseName);
        DropDatabase(_notificationDatabaseName);
        DropDatabase(_referenceDataDatabaseName);
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
