using GenclikMerkezi.IntegrationTests.Identity;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Identity/Notification/ReferenceData/CareerAdvisor stay real (so the User-creation half of
// RegisterEmployerCommand genuinely succeeds), but the Employer connection points at a LocalDB
// instance name that does not exist - any real operation against EmployerDbContext throws a real
// SqlException, without mocking anything. Used to verify RegisterEmployerCommandHandler's
// compensating DeactivateUserAsync call against a genuine infrastructure failure (same approach as
// CareerAdvisor's BrokenCareerAdvisorDatabaseWebApplicationFactory).
public sealed class BrokenEmployerDatabaseWebApplicationFactory : CustomWebApplicationFactory
{
    protected override string EmployerConnectionString =>
        "Server=(localdb)\\GenclikMerkezi-Nonexistent-Instance;Database=GenclikMerkezi.Employer.Broken;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2;";

    protected override void EnsureEmployerDatabaseCreated(IServiceProvider services)
    {
        // Deliberately does not call Database.EnsureCreated() - the connection is unreachable, so
        // doing that here would throw at host startup, before any test using this factory gets to
        // run. The failure needs to surface later, from a real request's SaveChangesAsync.
    }
}
