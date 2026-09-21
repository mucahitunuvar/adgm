using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employer.Domain;

// JobMilitaryStatusPreference ile aynı desen: MilitaryStatusId ReferenceData'nın MilitaryStatus
// lookup'ına referans verir, write-time'da doğrulanmaz (ADR-016).
public sealed class PersonnelNeedMilitaryStatusPreference : Entity
{
    public Guid PersonnelNeedId { get; private set; }

    public Guid MilitaryStatusId { get; private set; }

    private PersonnelNeedMilitaryStatusPreference(Guid id, Guid personnelNeedId, Guid militaryStatusId)
        : base(id)
    {
        PersonnelNeedId = personnelNeedId;
        MilitaryStatusId = militaryStatusId;
    }

    internal static PersonnelNeedMilitaryStatusPreference Create(Guid personnelNeedId, Guid militaryStatusId) =>
        new(Guid.NewGuid(), personnelNeedId, militaryStatusId);
}
