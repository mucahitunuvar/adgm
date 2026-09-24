using GenclikMerkezi.Modules.Candidate.Infrastructure;
using GenclikMerkezi.Modules.CareerAdvisor.Infrastructure;
using GenclikMerkezi.Modules.CareerDevelopment.Infrastructure;
using GenclikMerkezi.Modules.Employer.Infrastructure;
using GenclikMerkezi.Modules.Employment.Infrastructure;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.Modules.Interview.Infrastructure;
using GenclikMerkezi.Modules.Matching.Infrastructure;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Infrastructure;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GenclikMerkezi.IntegrationTests.Identity;

// All three databases are real (throwaway, per-test-run) LocalDB databases rather than ADR-012's
// usual Sqlite switch: IdentityDbContext is the CAP transactional outbox anchor (Program.cs's
// AddMessaging<IdentityDbContext>() call - CAP only supports one instance per process, see
// ADR-014's amendment), and CAP's SqlServer storage package cannot target a Sqlite connection.
// ReferenceData has no such constraint (ADR-012's Sqlite switch would work for it) but uses
// LocalDB too here, simply for consistency with the other two in this shared test factory.
//
// Every module added to this shared factory after those three was wired to LocalDB too, by
// copying this file's existing pattern rather than re-deriving it - none of them (Candidate
// through CareerDevelopment) has Identity/ReferenceData's stated reason and none is a CAP outbox
// anchor. That is a pre-existing, repo-wide deviation from ADR-012 ("Testing -> Sqlite"), out of
// scope to fix here. WebsiteDbContext is deliberately NOT added to that drift: it uses a real
// Sqlite in-memory connection (see _websiteSqliteConnection below), matching what ADR-012 actually
// specifies, since Website has no CAP/outbox constraint that would justify LocalDB.
//
// Not sealed: BrokenCandidateDatabaseWebApplicationFactory (Candidate's registration-compensation
// test) subclasses this to point only the Candidate connection at an unreachable server, reusing
// everything else here rather than duplicating LocalDB provisioning/cleanup for one test.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string LocalDbServer = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly string _identityDatabaseName = $"GenclikMerkezi.Identity.Test.{Guid.NewGuid():N}";
    private readonly string _notificationDatabaseName = $"GenclikMerkezi.Notification.Test.{Guid.NewGuid():N}";
    private readonly string _referenceDataDatabaseName = $"GenclikMerkezi.ReferenceData.Test.{Guid.NewGuid():N}";
    private readonly string _candidateDatabaseName = $"GenclikMerkezi.Candidate.Test.{Guid.NewGuid():N}";
    private readonly string _careerAdvisorDatabaseName = $"GenclikMerkezi.CareerAdvisor.Test.{Guid.NewGuid():N}";
    private readonly string _employerDatabaseName = $"GenclikMerkezi.Employer.Test.{Guid.NewGuid():N}";
    private readonly string _matchingDatabaseName = $"GenclikMerkezi.Matching.Test.{Guid.NewGuid():N}";
    private readonly string _interviewDatabaseName = $"GenclikMerkezi.Interview.Test.{Guid.NewGuid():N}";
    private readonly string _employmentDatabaseName = $"GenclikMerkezi.Employment.Test.{Guid.NewGuid():N}";
    private readonly string _careerDevelopmentDatabaseName = $"GenclikMerkezi.CareerDevelopment.Test.{Guid.NewGuid():N}";

    // ADR-012: Website has no CAP/outbox constraint, so (unlike every connection string above) it
    // uses a real Sqlite in-memory database instead of LocalDB. A Sqlite ":memory:" database is
    // destroyed the moment its connection closes, so one already-open connection is kept alive for
    // the factory's lifetime and handed to every WebsiteDbContext instance (see ConfigureWebHost).
    private readonly SqliteConnection _websiteSqliteConnection = CreateOpenWebsiteSqliteConnection();

    private static SqliteConnection CreateOpenWebsiteSqliteConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private string IdentityConnectionString =>
        $"{LocalDbServer}Database={_identityDatabaseName};";

    private string NotificationConnectionString =>
        $"{LocalDbServer}Database={_notificationDatabaseName};";

    private string ReferenceDataConnectionString =>
        $"{LocalDbServer}Database={_referenceDataDatabaseName};";

    // Overridable so BrokenCandidateDatabaseWebApplicationFactory can point this at an unreachable
    // server instead, to test RegisterCandidateCommand's compensation path against a real
    // (deliberately broken) connection rather than a mock.
    protected virtual string CandidateConnectionString =>
        $"{LocalDbServer}Database={_candidateDatabaseName};";

    // Overridable so BrokenCareerAdvisorDatabaseWebApplicationFactory can point this at an
    // unreachable server instead, to test CreateCareerAdvisorCommand's compensation path against a
    // real (deliberately broken) connection rather than a mock.
    protected virtual string CareerAdvisorConnectionString =>
        $"{LocalDbServer}Database={_careerAdvisorDatabaseName};";

    // Overridable so BrokenEmployerDatabaseWebApplicationFactory can point this at an unreachable
    // server instead, to test RegisterEmployerCommand's compensation path against a real
    // (deliberately broken) connection rather than a mock.
    protected virtual string EmployerConnectionString =>
        $"{LocalDbServer}Database={_employerDatabaseName};";

    private string MatchingConnectionString =>
        $"{LocalDbServer}Database={_matchingDatabaseName};";

    private string InterviewConnectionString =>
        $"{LocalDbServer}Database={_interviewDatabaseName};";

    private string EmploymentConnectionString =>
        $"{LocalDbServer}Database={_employmentDatabaseName};";

    private string CareerDevelopmentConnectionString =>
        $"{LocalDbServer}Database={_careerDevelopmentDatabaseName};";

    public FakeEmailSender EmailSender { get; } = new();

    public FakeBotProtectionVerifier BotProtectionVerifier { get; } = new();

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
        builder.UseSetting("ConnectionStrings:CandidateDatabase", CandidateConnectionString);
        builder.UseSetting("ConnectionStrings:CareerAdvisorDatabase", CareerAdvisorConnectionString);
        builder.UseSetting("ConnectionStrings:EmployerDatabase", EmployerConnectionString);
        builder.UseSetting("ConnectionStrings:MatchingDatabase", MatchingConnectionString);
        builder.UseSetting("ConnectionStrings:InterviewDatabase", InterviewConnectionString);
        builder.UseSetting("ConnectionStrings:EmploymentDatabase", EmploymentConnectionString);
        builder.UseSetting("ConnectionStrings:CareerDevelopmentDatabase", CareerDevelopmentConnectionString);
        builder.UseSetting("Jwt:Issuer", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:Audience", "GenclikMerkezi.Tests");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-do-not-use-in-prod");
        builder.UseSetting("Jwt:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("Jwt:RefreshTokenExpirationDays", "7");

        builder.ConfigureServices(services =>
        {
            // Overrides Program.cs's own AddWebsiteModule() registration (which points at
            // SqlServer, since Database:Provider is never set to "Sqlite" for this shared
            // factory's other modules - see the class-level comment). AddDbContext<TContext>
            // registers the configuration action as an IDbContextOptionsConfiguration<TContext>
            // entry (added, not replaced) rather than replacing DbContextOptions<TContext>
            // outright - removing only the DbContextOptions<TContext> singleton still left both
            // Program.cs's UseSqlServer() and this UseSqlite() call applied to the rebuilt options
            // ("Multiple relational database provider configurations found"). Both registration
            // kinds must be removed. UseInternalServiceProvider() then isolates WebsiteDbContext
            // from the app's shared ambient container, which still has SqlServer's provider
            // services registered for every other module ("Only a single database provider can be
            // registered in a service provider" otherwise).
            services.RemoveAll<DbContextOptions<WebsiteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<WebsiteDbContext>>();

            var websiteSqliteServiceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            services.AddDbContext<WebsiteDbContext>(options => options
                .UseSqlite(_websiteSqliteConnection)
                .UseInternalServiceProvider(websiteSqliteServiceProvider));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>().Database.EnsureCreated();
            EnsureCandidateDatabaseCreated(scope.ServiceProvider);
            EnsureCareerAdvisorDatabaseCreated(scope.ServiceProvider);
            EnsureEmployerDatabaseCreated(scope.ServiceProvider);
            scope.ServiceProvider.GetRequiredService<MatchingDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<InterviewDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<EmploymentDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<CareerDevelopmentDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<WebsiteDbContext>().Database.EnsureCreated();
        });

        // Runs after Program.cs's own registrations, so this replaces the real SmtpEmailSender (no
        // real mail server involved) and the real CloudflareTurnstileBotProtectionVerifier (no real
        // Cloudflare call involved) in these tests.
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEmailSender>(EmailSender);
            services.AddSingleton<IBotProtectionVerifier>(BotProtectionVerifier);
        });
    }

    // Overridden by BrokenCandidateDatabaseWebApplicationFactory to no-op: EnsureCreated against an
    // unreachable server would throw here, at host startup, before any test gets to run - the point
    // of that factory is for the failure to surface later, from a real request's SaveChangesAsync.
    protected virtual void EnsureCandidateDatabaseCreated(IServiceProvider services)
    {
        services.GetRequiredService<CandidateDbContext>().Database.EnsureCreated();
    }

    // Overridden by BrokenCareerAdvisorDatabaseWebApplicationFactory to no-op, same reasoning as
    // EnsureCandidateDatabaseCreated above.
    protected virtual void EnsureCareerAdvisorDatabaseCreated(IServiceProvider services)
    {
        services.GetRequiredService<CareerAdvisorDbContext>().Database.EnsureCreated();
    }

    // Overridden by BrokenEmployerDatabaseWebApplicationFactory to no-op, same reasoning as
    // EnsureCandidateDatabaseCreated above.
    protected virtual void EnsureEmployerDatabaseCreated(IServiceProvider services)
    {
        services.GetRequiredService<EmployerDbContext>().Database.EnsureCreated();
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
            PasswordHash.FromHashedValue(passwordHasher.Hash(password)), "Test", "User", null,
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
        DropDatabase(_candidateDatabaseName);
        DropDatabase(_careerAdvisorDatabaseName);
        DropDatabase(_employerDatabaseName);
        DropDatabase(_matchingDatabaseName);
        DropDatabase(_interviewDatabaseName);
        DropDatabase(_employmentDatabaseName);
        DropDatabase(_careerDevelopmentDatabaseName);
        _websiteSqliteConnection.Dispose();
    }

    // Best-effort cleanup of the throwaway LocalDB databases - EF Core's connection pool may
    // still hold a pooled (but idle) connection open at this point, so this can occasionally
    // fail; that would only leak one small test-run-specific database, not fail the test run.
    // Also harmless (and expected) to no-op for a database that was never actually created, e.g.
    // BrokenCandidateDatabaseWebApplicationFactory's deliberately-unreachable connection.
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
