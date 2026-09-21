using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// GenderId, ReferenceData'nın Gender lookup'ına referans verir, write-time'da doğrulanmaz (ADR-016) -
// CandidateLanguage.cs ile aynı desen.
public sealed class JobGenderPreference : Entity
{
    public Guid JobId { get; private set; }

    public Guid GenderId { get; private set; }

    private JobGenderPreference(Guid id, Guid jobId, Guid genderId)
        : base(id)
    {
        JobId = jobId;
        GenderId = genderId;
    }

    internal static JobGenderPreference Create(Guid jobId, Guid genderId) => new(Guid.NewGuid(), jobId, genderId);
}
