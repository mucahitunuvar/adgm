using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// JobEducationLevelPreference ile aynı desen: EducationLevelId ReferenceData'nın EducationLevel
// lookup'ına referans verir, write-time'da doğrulanmaz (ADR-016).
public sealed class PersonnelNeedEducationLevelPreference : Entity
{
    public Guid PersonnelNeedId { get; private set; }

    public Guid EducationLevelId { get; private set; }

    private PersonnelNeedEducationLevelPreference(Guid id, Guid personnelNeedId, Guid educationLevelId)
        : base(id)
    {
        PersonnelNeedId = personnelNeedId;
        EducationLevelId = educationLevelId;
    }

    internal static PersonnelNeedEducationLevelPreference Create(Guid personnelNeedId, Guid educationLevelId) =>
        new(Guid.NewGuid(), personnelNeedId, educationLevelId);
}
