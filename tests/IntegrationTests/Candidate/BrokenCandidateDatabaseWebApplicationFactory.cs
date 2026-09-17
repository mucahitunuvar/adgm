using GenclikMerkezi.IntegrationTests.Identity;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// Identity/Notification/ReferenceData stay real (so the User-creation half of
// RegisterCandidateCommand genuinely succeeds), but the Candidate connection points at a
// LocalDB instance name that does not exist - any real operation against CandidateDbContext
// (the CandidateCv/CandidateCvContent SaveChangesAsync) throws a real SqlException, without
// mocking anything. Used to verify RegisterCandidateCommandHandler's compensating
// DeactivateUserAsync call against a genuine infrastructure failure (Candidate module master
// prompt Görev 5: "gerçek bir hata enjekte ederek test et, mock'lamadan kaçın").
public sealed class BrokenCandidateDatabaseWebApplicationFactory : CustomWebApplicationFactory
{
    protected override string CandidateConnectionString =>
        "Server=(localdb)\\GenclikMerkezi-Nonexistent-Instance;Database=GenclikMerkezi.Candidate.Broken;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2;";

    protected override void EnsureCandidateDatabaseCreated(IServiceProvider services)
    {
        // Deliberately does not call Database.EnsureCreated() - the connection is unreachable, so
        // doing that here would throw at host startup, before any test using this factory gets to
        // run. The failure needs to surface later, from a real request's SaveChangesAsync.
    }
}
