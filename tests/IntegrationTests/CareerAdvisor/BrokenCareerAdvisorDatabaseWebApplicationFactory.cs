using GenclikMerkezi.IntegrationTests.Identity;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Identity/Notification/ReferenceData/Candidate stay real (so the User-creation half of
// CreateCareerAdvisorCommand genuinely succeeds), but the CareerAdvisor connection points at a
// LocalDB instance name that does not exist - any real operation against CareerAdvisorDbContext
// throws a real SqlException, without mocking anything. Used to verify
// CreateCareerAdvisorCommandHandler's compensating DeactivateUserAsync call against a genuine
// infrastructure failure (same approach as Candidate's BrokenCandidateDatabaseWebApplicationFactory).
public sealed class BrokenCareerAdvisorDatabaseWebApplicationFactory : CustomWebApplicationFactory
{
    protected override string CareerAdvisorConnectionString =>
        "Server=(localdb)\\GenclikMerkezi-Nonexistent-Instance;Database=GenclikMerkezi.CareerAdvisor.Broken;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2;";

    protected override void EnsureCareerAdvisorDatabaseCreated(IServiceProvider services)
    {
        // Deliberately does not call Database.EnsureCreated() - the connection is unreachable, so
        // doing that here would throw at host startup, before any test using this factory gets to
        // run. The failure needs to surface later, from a real request's SaveChangesAsync.
    }
}
