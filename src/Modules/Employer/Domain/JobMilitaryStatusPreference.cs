using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// MilitaryStatusId, ReferenceData'nın MilitaryStatus lookup'ına referans verir, write-time'da
// doğrulanmaz (ADR-016) - CandidateLanguage.cs ile aynı desen.
public sealed class JobMilitaryStatusPreference : Entity
{
    public Guid JobId { get; private set; }

    public Guid MilitaryStatusId { get; private set; }

    private JobMilitaryStatusPreference(Guid id, Guid jobId, Guid militaryStatusId)
        : base(id)
    {
        JobId = jobId;
        MilitaryStatusId = militaryStatusId;
    }

    internal static JobMilitaryStatusPreference Create(Guid jobId, Guid militaryStatusId) =>
        new(Guid.NewGuid(), jobId, militaryStatusId);
}
