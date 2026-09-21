using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// EducationLevelId, ReferenceData'nın EducationLevel lookup'ına referans verir, write-time'da
// doğrulanmaz (ADR-016) - CandidateLanguage.cs ile aynı desen.
public sealed class JobEducationLevelPreference : Entity
{
    public Guid JobId { get; private set; }

    public Guid EducationLevelId { get; private set; }

    private JobEducationLevelPreference(Guid id, Guid jobId, Guid educationLevelId)
        : base(id)
    {
        JobId = jobId;
        EducationLevelId = educationLevelId;
    }

    internal static JobEducationLevelPreference Create(Guid jobId, Guid educationLevelId) =>
        new(Guid.NewGuid(), jobId, educationLevelId);
}
