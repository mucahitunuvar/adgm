using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 4): backs the yearly reference-number counter
// ("{prefix}-{year}-{6 haneli sıra}") - one row per year, RowVersion guards concurrent reservations
// the same application-managed optimistic-concurrency way every other Website aggregate does (no
// SQL Server SEQUENCE, since the module also runs on Sqlite for integration tests, ADR-012). The
// Application-layer command handler is responsible for retrying (reload + re-reserve) when a
// concurrent reservation wins the race - see SubmitFormSubmissionCommandHandler's remarks.
public sealed class FormSubmissionSequence : AggregateRoot
{
    public int Year { get; private set; }

    public int NextValue { get; private set; }

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    private FormSubmissionSequence(Guid id, int year)
        : base(id)
    {
        Year = year;
        NextValue = 0;
    }

    private FormSubmissionSequence()
    {
    }

    public static FormSubmissionSequence CreateForYear(int year) => new(Guid.NewGuid(), year);

    // Reserves and returns the next sequential value for this year, bumping RowVersion so a
    // concurrent reservation against the same stale read loses the race (caught by the handler's
    // retry loop, not here - this method has no repository/persistence access of its own).
    public int Reserve()
    {
        NextValue++;
        RowVersion = Guid.NewGuid().ToByteArray();

        return NextValue;
    }
}
